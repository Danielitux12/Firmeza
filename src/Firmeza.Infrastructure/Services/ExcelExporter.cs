using System.Drawing;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Firmeza.Infrastructure.Services;

/// <summary>
/// Exporta listados de productos, clientes y ventas hacia hojas de cálculo en formato Excel (.xlsx).
/// </summary>
public class ExcelExporter : IExcelExporter
{
    public ExcelExporter()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    // Exporta el catálogo completo de productos a formato Excel.
    public byte[] ExportProducts(IEnumerable<Product> products)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Productos");

        // Encabezados
        string[] headers = ["ID", "Nombre", "Categoría", "Precio ($)", "Stock", "Disponible", "Descripción"];
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
        }

        ApplyHeaderStyle(ws, headers.Length);

        // Filas de datos
        int row = 2;
        foreach (var p in products)
        {
            ws.Cells[row, 1].Value = p.Id;
            ws.Cells[row, 2].Value = p.Name;
            ws.Cells[row, 3].Value = p.Category;
            ws.Cells[row, 4].Value = p.Price;
            ws.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";
            ws.Cells[row, 5].Value = p.Stock;
            ws.Cells[row, 6].Value = p.IsAvailable ? "Sí" : "No";
            ws.Cells[row, 7].Value = p.Description;
            row++;
        }

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    // Exporta el directorio de clientes a formato Excel.
    public byte[] ExportCustomers(IEnumerable<Customer> customers)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Clientes");

        // Encabezados
        string[] headers = ["ID", "Nombre Completo", "Documento", "Correo Electrónico", "Teléfono", "Dirección"];
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
        }

        ApplyHeaderStyle(ws, headers.Length);

        // Filas de datos
        int row = 2;
        foreach (var c in customers)
        {
            ws.Cells[row, 1].Value = c.Id;
            ws.Cells[row, 2].Value = c.FullName;
            ws.Cells[row, 3].Value = c.DocumentNumber;
            ws.Cells[row, 4].Value = c.Email;
            ws.Cells[row, 5].Value = c.Phone;
            ws.Cells[row, 6].Value = c.Address;
            row++;
        }

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    // Exporta el historial de ventas con sus importes e impuestos a formato Excel.
    public byte[] ExportSales(IEnumerable<Sale> sales)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Ventas");

        // Encabezados
        string[] headers = ["ID", "N° Venta", "Fecha", "Cliente", "Documento", "Subtotal ($)", "IVA 19% ($)", "Total ($)"];
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
        }

        ApplyHeaderStyle(ws, headers.Length);

        // Filas de datos
        int row = 2;
        foreach (var s in sales)
        {
            ws.Cells[row, 1].Value = s.Id;
            ws.Cells[row, 2].Value = s.SaleNumber;
            ws.Cells[row, 3].Value = s.Date.ToString("yyyy-MM-dd HH:mm");
            ws.Cells[row, 4].Value = s.Customer?.FullName ?? "N/A";
            ws.Cells[row, 5].Value = s.Customer?.DocumentNumber ?? "-";
            ws.Cells[row, 6].Value = s.Subtotal;
            ws.Cells[row, 6].Style.Numberformat.Format = "$#,##0.00";
            ws.Cells[row, 7].Value = s.Tax;
            ws.Cells[row, 7].Style.Numberformat.Format = "$#,##0.00";
            ws.Cells[row, 8].Value = s.Total;
            ws.Cells[row, 8].Style.Numberformat.Format = "$#,##0.00";
            row++;
        }

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    // Aplica color de fondo azul, texto blanco en negrita y bordes a los encabezados.
    private void ApplyHeaderStyle(ExcelWorksheet ws, int columnsCount)
    {
        using var range = ws.Cells[1, 1, 1, columnsCount];
        range.Style.Font.Bold = true;
        range.Style.Font.Color.SetColor(Color.White);
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(15, 23, 42)); // Azul oscuro
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    }
}
