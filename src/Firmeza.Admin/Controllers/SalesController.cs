using Firmeza.Application.Interfaces;
using Firmeza.Application.ViewModels.Sales;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Admin.Controllers;

/// <summary>
/// Gestiona la visualización y el registro de ventas con cálculo de impuestos, generación de recibos PDF y exportaciones.
/// </summary>
[Authorize(Roles = "Administrador")]
public class SalesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IExcelExporter _excelExporter;
    private readonly IPdfExporter _pdfExporter;
    private readonly IReceiptGenerator _receiptGenerator;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SalesController(
        AppDbContext context,
        IExcelExporter excelExporter,
        IPdfExporter pdfExporter,
        IReceiptGenerator receiptGenerator,
        IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
        _receiptGenerator = receiptGenerator;
        _webHostEnvironment = webHostEnvironment;
    }

    // Muestra el historial completo de ventas realizadas.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var sales = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
            .OrderByDescending(s => s.Date)
            .Select(s => new SaleListItemViewModel
            {
                Id = s.Id,
                SaleNumber = s.SaleNumber,
                Date = s.Date,
                CustomerName = s.Customer != null ? s.Customer.FullName : "Cliente no registrado",
                CustomerDocument = s.Customer != null ? s.Customer.DocumentNumber : "-",
                TotalItems = s.SaleDetails.Sum(sd => sd.Quantity),
                Subtotal = s.Subtotal,
                Tax = s.Tax,
                Total = s.Total,
                ReceiptPath = s.ReceiptPath
            })
            .ToListAsync();

        return View(sales);
    }

    // Muestra el detalle específico de una venta con cada ítem y los totales calculados.
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var sale = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
            .ThenInclude(sd => sd.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null)
        {
            return NotFound();
        }

        var model = new SaleDetailsViewModel
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            Date = sale.Date,
            CustomerName = sale.Customer?.FullName ?? "N/A",
            CustomerDocument = sale.Customer?.DocumentNumber ?? "N/A",
            CustomerEmail = sale.Customer?.Email ?? "N/A",
            CustomerPhone = sale.Customer?.Phone ?? "N/A",
            Subtotal = sale.Subtotal,
            Tax = sale.Tax,
            Total = sale.Total,
            ReceiptPath = sale.ReceiptPath,
            Items = sale.SaleDetails.Select(sd => new SaleDetailItemViewModel
            {
                ProductId = sd.ProductId,
                ProductName = sd.Product?.Name ?? "Producto eliminado",
                Quantity = sd.Quantity,
                UnitPrice = sd.UnitPrice,
                LineTotal = sd.LineTotal
            }).ToList()
        };

        return View(model);
    }

    // Muestra el formulario para registrar una nueva venta con la lista de clientes y productos.
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new SaleCreateViewModel();
        await PopulateSelectListsAsync(model);
        return View(model);
    }

    // Procesa el registro de la venta, valida existencias, descuenta stock y calcula IVA (19%).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaleCreateViewModel model)
    {
        // 1. Filtra los ítems válidos (con producto y cantidad > 0).
        var validItems = model.Items
            .Where(i => i.ProductId > 0 && i.Quantity > 0)
            .ToList();

        if (validItems.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Debes agregar al menos un producto a la venta con cantidad mayor a 0.");
        }

        // 2. Valida que el cliente exista.
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == model.CustomerId);
        if (!customerExists)
        {
            ModelState.AddModelError(nameof(model.CustomerId), "El cliente seleccionado no existe.");
        }

        // 3. Valida el stock disponible de cada producto solicitado.
        var productIds = validItems.Select(i => i.ProductId).Distinct().ToList();
        var productsInDb = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var item in validItems)
        {
            if (!productsInDb.TryGetValue(item.ProductId, out var product))
            {
                ModelState.AddModelError(string.Empty, $"El producto con ID {item.ProductId} no fue encontrado.");
                continue;
            }

            if (!product.IsAvailable)
            {
                ModelState.AddModelError(string.Empty, $"El producto '{product.Name}' no está disponible para venta.");
            }
            else if (product.Stock < item.Quantity)
            {
                ModelState.AddModelError(string.Empty, $"Stock insuficiente para '{product.Name}'. Disponible: {product.Stock}, Solicitado: {item.Quantity}.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(model);
            return View(model);
        }

        // 4. Genera el número correlativo único de la venta.
        var currentCount = await _context.Sales.CountAsync() + 1;
        var saleNumber = $"VTA-{currentCount:D5}";

        var sale = new Sale
        {
            SaleNumber = saleNumber,
            CustomerId = model.CustomerId,
            Date = DateTime.UtcNow
        };

        // 5. Crea los detalles y descuenta el stock de cada producto.
        foreach (var item in validItems)
        {
            var product = productsInDb[item.ProductId];

            // Descuenta del stock físico y actualiza disponibilidad si llega a cero.
            product.ReduceStock(item.Quantity);

            var detail = new SaleDetail
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };
            detail.CalculateLineTotal();

            sale.SaleDetails.Add(detail);
        }

        // 6. Calcula Subtotal, IVA (19%) y Total.
        sale.CalculateTotals();

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        // 7. Genera el recibo físico en formato PDF y guarda la ruta en Sale.ReceiptPath
        try
        {
            // Asegura que las propiedades de navegación de cliente y producto estén enlazadas
            sale.Customer = await _context.Customers.FindAsync(sale.CustomerId);
            string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var receiptPath = await _receiptGenerator.GenerateReceiptPdfAsync(sale, webRoot);
            sale.ReceiptPath = receiptPath;
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Si ocurre un error al generar el PDF no interrumpe el registro de la venta en base de datos.
        }

        TempData["SuccessMessage"] = $"¡Venta {sale.SaleNumber} registrada con éxito! Total a pagar: ${sale.Total:N2}. Se generó el recibo en PDF.";
        return RedirectToAction(nameof(Details), new { id = sale.Id });
    }

    // Exporta el historial de ventas a formato Excel (.xlsx).
    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var sales = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        var fileBytes = _excelExporter.ExportSales(sales);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Ventas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    // Exporta el historial de ventas a documento PDF.
    [HttpGet]
    public async Task<IActionResult> ExportPdf()
    {
        var sales = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        var fileBytes = _pdfExporter.ExportSales(sales);
        return File(fileBytes, "application/pdf", $"Ventas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    // Permite descargar el comprobante en PDF de una venta específica.
    [HttpGet]
    public async Task<IActionResult> DownloadReceipt(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
            .ThenInclude(sd => sd.Product)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null)
        {
            return NotFound();
        }

        string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        // Si el recibo no existe físicamente en disco, se genera en caliente.
        string physicalPath = Path.Combine(webRoot, sale.ReceiptPath?.TrimStart('/') ?? $"recibos/recibo_{sale.SaleNumber}.pdf");
        if (!System.IO.File.Exists(physicalPath))
        {
            var path = await _receiptGenerator.GenerateReceiptPdfAsync(sale, webRoot);
            sale.ReceiptPath = path;
            await _context.SaveChangesAsync();
            physicalPath = Path.Combine(webRoot, path.TrimStart('/'));
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
        return File(fileBytes, "application/pdf", $"Recibo_{sale.SaleNumber}.pdf");
    }

    // Carga los clientes y productos disponibles para poblar los selectores del formulario.
    private async Task PopulateSelectListsAsync(SaleCreateViewModel model)
    {
        model.AvailableCustomers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .Select(c => new CustomerOptionViewModel
            {
                Id = c.Id,
                DisplayText = $"{c.FullName} - Doc: {c.DocumentNumber}"
            })
            .ToListAsync();

        model.AvailableProducts = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsAvailable && p.Stock > 0)
            .OrderBy(p => p.Name)
            .Select(p => new ProductOptionViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock
            })
            .ToListAsync();
    }
}
