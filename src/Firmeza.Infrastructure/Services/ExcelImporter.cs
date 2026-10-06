using System.Globalization;
using System.Text;
using Firmeza.Application.Common;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Importa y normaliza datos provenientes de archivos Excel desnormalizados (.xlsx).
/// Separa en memoria clientes, productos y ventas antes de persistir.
/// </summary>
public class ExcelImporter : IExcelImporter
{
    private readonly AppDbContext _context;

    public ExcelImporter(AppDbContext context)
    {
        _context = context;
        // Configura la licencia no comercial de EPPlus requerida por versiones recientes.
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    // Lee el Excel, valida las filas, normaliza entidades y guarda los datos en la base de datos.
    public async Task<ImportResult> ImportAsync(Stream fileStream, string logDirectory, CancellationToken cancellationToken = default)
    {
        var result = new ImportResult();

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet is null || worksheet.Dimension is null)
        {
            result.Errors.Add(new ImportRowError
            {
                RowNumber = 1,
                ColumnName = "Hoja",
                ErrorMessage = "El archivo no contiene hojas con datos legibles."
            });
            await GenerateLogFileAsync(result, logDirectory);
            return result;
        }

        int totalRows = worksheet.Dimension.Rows;
        int totalCols = worksheet.Dimension.Columns;

        // 1. Mapea encabezados a posiciones de columnas para ser tolerante a variaciones en nombres.
        var colMap = MapHeaders(worksheet, totalCols);

        // Verifica que existan las columnas mínimas indispensables.
        if (!colMap.ContainsKey("cliente") || !colMap.ContainsKey("documento") ||
            !colMap.ContainsKey("producto") || !colMap.ContainsKey("precio") ||
            !colMap.ContainsKey("cantidad"))
        {
            result.Errors.Add(new ImportRowError
            {
                RowNumber = 1,
                ColumnName = "Encabezados",
                ErrorMessage = "Faltan columnas obligatorias. Se requiere: Cliente, Documento, Producto, Precio y Cantidad."
            });
            await GenerateLogFileAsync(result, logDirectory);
            return result;
        }

        // Carga clientes y productos existentes en memoria para optimizar búsquedas e inserción/actualización.
        var existingCustomers = await _context.Customers.ToDictionaryAsync(c => c.DocumentNumber.Trim(), cancellationToken);
        var existingProducts = await _context.Products.ToDictionaryAsync(p => p.Name.Trim().ToLower(), cancellationToken);

        // Agrupador en memoria de ventas por (DocumentoCliente, Fecha, NumeroVenta)
        var salesBuffer = new Dictionary<string, Sale>();

        // 2. Procesa cada fila a partir de la fila 2 (la fila 1 son encabezados).
        for (int row = 2; row <= totalRows; row++)
        {
            // Salta filas totalmente vacías.
            if (IsRowEmpty(worksheet, row, totalCols))
            {
                continue;
            }

            result.TotalRowsRead++;
            bool rowHasError = false;

            // --- Lectura y validación de Cliente ---
            string customerName = GetCellString(worksheet, row, colMap, "cliente");
            string documentNumber = GetCellString(worksheet, row, colMap, "documento");
            string email = GetCellString(worksheet, row, colMap, "email");
            string phone = GetCellString(worksheet, row, colMap, "telefono");
            string address = GetCellString(worksheet, row, colMap, "direccion");

            if (string.IsNullOrWhiteSpace(customerName))
            {
                result.Errors.Add(new ImportRowError { RowNumber = row, ColumnName = "Cliente", ErrorMessage = "El nombre del cliente es obligatorio." });
                rowHasError = true;
            }

            if (string.IsNullOrWhiteSpace(documentNumber))
            {
                result.Errors.Add(new ImportRowError { RowNumber = row, ColumnName = "Documento", ErrorMessage = "El documento del cliente es obligatorio." });
                rowHasError = true;
            }

            // --- Lectura y validación de Producto ---
            string productName = GetCellString(worksheet, row, colMap, "producto");
            string description = GetCellString(worksheet, row, colMap, "descripcion");
            string category = GetCellString(worksheet, row, colMap, "categoria");
            if (string.IsNullOrWhiteSpace(category)) category = "General";

            string priceStr = GetCellString(worksheet, row, colMap, "precio");
            string qtyStr = GetCellString(worksheet, row, colMap, "cantidad");
            string stockStr = GetCellString(worksheet, row, colMap, "stock");

            if (string.IsNullOrWhiteSpace(productName))
            {
                result.Errors.Add(new ImportRowError { RowNumber = row, ColumnName = "Producto", ErrorMessage = "El nombre del producto es obligatorio." });
                rowHasError = true;
            }

            if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price <= 0)
            {
                result.Errors.Add(new ImportRowError { RowNumber = row, ColumnName = "Precio", ErrorMessage = "El precio debe ser un número decimal mayor a 0." });
                rowHasError = true;
            }

            if (!int.TryParse(qtyStr, out int quantity) || quantity <= 0)
            {
                result.Errors.Add(new ImportRowError { RowNumber = row, ColumnName = "Cantidad", ErrorMessage = "La cantidad vendida debe ser un entero mayor a 0." });
                rowHasError = true;
            }

            int stock = 0;
            if (!string.IsNullOrWhiteSpace(stockStr))
            {
                int.TryParse(stockStr, out stock);
            }

            // --- Lectura de Venta ---
            string saleNumber = GetCellString(worksheet, row, colMap, "numeroventa");
            string dateStr = GetCellString(worksheet, row, colMap, "fecha");
            DateTime saleDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dateStr) && DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                saleDate = parsedDate;
            }

            // Si la fila tiene errores, no se procesa en la base de datos pero se continúa con las demás.
            if (rowHasError)
            {
                continue;
            }

            // 3. Normalizar o actualizar Cliente por DocumentNumber
            documentNumber = documentNumber.Trim();
            if (!existingCustomers.TryGetValue(documentNumber, out var customer))
            {
                // Crea nuevo cliente si no existe
                string validEmail = string.IsNullOrWhiteSpace(email) ? $"cliente_{documentNumber}@firmeza.com" : email.Trim();
                customer = new Customer
                {
                    FullName = customerName.Trim(),
                    DocumentNumber = documentNumber,
                    Email = validEmail,
                    Phone = phone?.Trim() ?? string.Empty,
                    Address = address?.Trim() ?? string.Empty
                };
                _context.Customers.Add(customer);
                existingCustomers[documentNumber] = customer;
                result.CustomersProcessed++;
            }
            else
            {
                // Actualiza información si venía en el archivo
                customer.FullName = customerName.Trim();
                if (!string.IsNullOrWhiteSpace(email)) customer.Email = email.Trim();
                if (!string.IsNullOrWhiteSpace(phone)) customer.Phone = phone.Trim();
                if (!string.IsNullOrWhiteSpace(address)) customer.Address = address.Trim();
            }

            // 4. Normalizar o actualizar Producto por Name
            string productKey = productName.Trim().ToLower();
            if (!existingProducts.TryGetValue(productKey, out var product))
            {
                product = new Product
                {
                    Name = productName.Trim(),
                    Description = description?.Trim() ?? string.Empty,
                    Category = category.Trim(),
                    Price = price,
                    Stock = stock > 0 ? stock : quantity * 2, // Si no venía stock, asegura suficiente
                    IsAvailable = true
                };
                _context.Products.Add(product);
                existingProducts[productKey] = product;
                result.ProductsProcessed++;
            }
            else
            {
                // Actualiza precio y categoría si cambiaron
                product.Price = price;
                if (!string.IsNullOrWhiteSpace(category)) product.Category = category.Trim();
                if (stock > 0) product.Stock = stock;
            }

            // 5. Agrupar o crear Venta asociada
            if (string.IsNullOrWhiteSpace(saleNumber))
            {
                saleNumber = $"IMP-{saleDate:yyyyMMdd}-{customer.DocumentNumber.Substring(0, Math.Min(4, customer.DocumentNumber.Length))}";
            }

            string saleBufferKey = $"{customer.DocumentNumber}_{saleNumber}".ToUpper();
            if (!salesBuffer.TryGetValue(saleBufferKey, out var sale))
            {
                sale = new Sale
                {
                    SaleNumber = saleNumber,
                    Customer = customer,
                    Date = saleDate
                };
                salesBuffer[saleBufferKey] = sale;
                _context.Sales.Add(sale);
                result.SalesCreated++;
            }

            // Agrega detalle de la venta
            var detail = new SaleDetail
            {
                Product = product,
                Quantity = quantity,
                UnitPrice = price,
                LineTotal = quantity * price
            };
            sale.SaleDetails.Add(detail);

            // Descuenta stock
            product.Stock = Math.Max(0, product.Stock - quantity);
            if (product.Stock == 0) product.IsAvailable = false;

            result.SuccessRows++;
        }

        // 6. Recalcula subtotales e impuestos para todas las ventas creadas en el lote.
        foreach (var sale in salesBuffer.Values)
        {
            sale.CalculateTotals();
        }

        // Guarda cambios en la base de datos si hubo filas correctas.
        if (result.SuccessRows > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Genera el archivo .txt de log con los resultados y detalle de errores.
        await GenerateLogFileAsync(result, logDirectory);

        return result;
    }

    // Detecta en qué índice de columna se encuentra cada propiedad por similitud de nombre.
    private Dictionary<string, int> MapHeaders(ExcelWorksheet worksheet, int totalCols)
    {
        var map = new Dictionary<string, int>();

        for (int col = 1; col <= totalCols; col++)
        {
            var header = worksheet.Cells[1, col].Text?.Trim().ToLower() ?? "";
            // Remueve tildes y caracteres especiales
            header = header.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n").Replace(" ", "").Replace("_", "");

            if (header.Contains("cliente") || header.Contains("nombrecliente")) map["cliente"] = col;
            else if (header.Contains("documento") || header.Contains("cedula") || header.Contains("dni") || header.Contains("nit")) map["documento"] = col;
            else if (header.Contains("email") || header.Contains("correo")) map["email"] = col;
            else if (header.Contains("telefono") || header.Contains("tel") || header.Contains("celular")) map["telefono"] = col;
            else if (header.Contains("direccion") || header.Contains("dir")) map["direccion"] = col;
            else if (header.Contains("producto") || header.Contains("articulo") || header.Contains("item")) map["producto"] = col;
            else if (header.Contains("descripcion") || header.Contains("detalle")) map["descripcion"] = col;
            else if (header.Contains("categoria") || header.Contains("rubro")) map["categoria"] = col;
            else if (header.Contains("precio") || header.Contains("valor")) map["precio"] = col;
            else if (header.Contains("stock") || header.Contains("inventario")) map["stock"] = col;
            else if (header.Contains("cantidad") || header.Contains("unidades") || header.Contains("cant")) map["cantidad"] = col;
            else if (header.Contains("fecha")) map["fecha"] = col;
            else if (header.Contains("venta") || header.Contains("factura") || header.Contains("comprobante")) map["numeroventa"] = col;
        }

        return map;
    }

    private string GetCellString(ExcelWorksheet ws, int row, Dictionary<string, int> map, string key)
    {
        if (map.TryGetValue(key, out int col))
        {
            return ws.Cells[row, col].Text?.Trim() ?? string.Empty;
        }
        return string.Empty;
    }

    private bool IsRowEmpty(ExcelWorksheet ws, int row, int totalCols)
    {
        for (int col = 1; col <= totalCols; col++)
        {
            if (!string.IsNullOrWhiteSpace(ws.Cells[row, col].Text)) return false;
        }
        return true;
    }

    // Escribe el log .txt en disco y guarda la ruta en el resultado.
    private async Task GenerateLogFileAsync(ImportResult result, string logDirectory)
    {
        try
        {
            Directory.CreateDirectory(logDirectory);
            string fileName = $"import_log_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
            string fullPath = Path.Combine(logDirectory, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("=====================================================");
            sb.AppendLine("           FIRMEZA - LOG DE IMPORTACIÓN EXCEL        ");
            sb.AppendLine("=====================================================");
            sb.AppendLine($"Fecha y Hora (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Total filas evaluadas: {result.TotalRowsRead}");
            sb.AppendLine($"Filas procesadas exitosamente: {result.SuccessRows}");
            sb.AppendLine($"Clientes creados/actualizados: {result.CustomersProcessed}");
            sb.AppendLine($"Productos creados/actualizados: {result.ProductsProcessed}");
            sb.AppendLine($"Ventas creadas: {result.SalesCreated}");
            sb.AppendLine($"Total de errores detectados: {result.Errors.Count}");
            sb.AppendLine("-----------------------------------------------------");

            if (result.Errors.Count == 0)
            {
                sb.AppendLine("¡Importación completada sin errores!");
            }
            else
            {
                sb.AppendLine("DETALLE DE ERRORES ENCONTRADOS:");
                foreach (var err in result.Errors)
                {
                    sb.AppendLine($"[Fila {err.RowNumber}] Columna '{err.ColumnName}': {err.ErrorMessage}");
                }
            }

            sb.AppendLine("=====================================================");
            await File.WriteAllTextAsync(fullPath, sb.ToString(), Encoding.UTF8);

            result.LogFilePath = $"/logs/{fileName}";
        }
        catch
        {
            // En caso de que no se pueda escribir en disco no bloqueamos la respuesta principal.
        }
    }
}
