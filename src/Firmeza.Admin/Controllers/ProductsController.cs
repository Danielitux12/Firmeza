using Firmeza.Application.Interfaces;
using Firmeza.Application.ViewModels.Products;
using Firmeza.Domain.Entities;
using Firmeza.Domain.Enums;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Admin.Controllers;

/// <summary>
/// Gestiona el catálogo de productos (CRUD), búsquedas y filtros para administradores.
/// </summary>
[Authorize(Roles = "Administrador")]
public class ProductsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IExcelExporter _excelExporter;
    private readonly IPdfExporter _pdfExporter;

    public ProductsController(
        AppDbContext context,
        IExcelExporter excelExporter,
        IPdfExporter pdfExporter)
    {
        _context = context;
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
    }

    // Muestra el listado de productos con opciones de búsqueda por nombre, filtro por categoría y disponibilidad.
    [HttpGet]
    public async Task<IActionResult> Index(string? searchName, string? selectedCategory, ProductStatus? statusFilter)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        // Filtro por nombre si se especificó.
        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var term = searchName.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        // Filtro por categoría seleccionada.
        if (!string.IsNullOrWhiteSpace(selectedCategory))
        {
            query = query.Where(p => p.Category == selectedCategory);
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(p => p.Status == statusFilter.Value);
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Category = p.Category,
                Status = p.Status
            })
            .ToListAsync();

        // Carga la lista de categorías existentes para el selector del filtro.
        var categories = await _context.Products
            .Select(p => p.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var model = new ProductFilterViewModel
        {
            SearchName = searchName,
            SelectedCategory = selectedCategory,
            StatusFilter = statusFilter,
            AvailableCategories = categories,
            Products = products
        };

        return View(model);
    }

    // Muestra el formulario para registrar un nuevo producto.
    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProductViewModel { Status = ProductStatus.Available });
    }

    // Guarda el nuevo producto en la base de datos tras validar los datos.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim() ?? string.Empty,
            Price = model.Price,
            Stock = model.Stock,
            Category = model.Category.Trim(),
            Status = model.Status == ProductStatus.Available && model.Stock == 0
                ? ProductStatus.Unavailable
                : model.Status
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"El producto '{product.Name}' fue registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Muestra el formulario de edición de un producto existente.
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Category = product.Category,
            Status = product.Status
        };

        return View(model);
    }

    // Guarda los cambios aplicados al producto.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        product.Name = model.Name.Trim();
        product.Description = model.Description?.Trim() ?? string.Empty;
        product.Price = model.Price;
        product.Stock = model.Stock;
        product.Category = model.Category.Trim();
        product.Status = model.Status == ProductStatus.Available && model.Stock == 0
            ? ProductStatus.Unavailable
            : model.Status;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"El producto '{product.Name}' fue actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Muestra la vista detallada de un producto.
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Category = product.Category,
            Status = product.Status
        };

        return View(model);
    }

    // Muestra la pantalla de confirmación para eliminar un producto.
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Category = product.Category,
            Status = product.Status
        };

        return View(model);
    }

    // Elimina el producto o lo desactiva si tiene ventas relacionadas.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        // Si el producto ya figura en detalles de ventas, no lo borramos físicamente para mantener integridad.
        var hasSales = await _context.SaleDetails.AnyAsync(sd => sd.ProductId == id);
        if (hasSales)
        {
            product.Status = ProductStatus.Unavailable;
            await _context.SaveChangesAsync();
            TempData["WarningMessage"] = $"El producto '{product.Name}' tiene ventas asociadas, por lo que fue marcado como 'No Disponible' en lugar de eliminarse.";
        }
        else
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"El producto '{product.Name}' fue eliminado exitosamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Exporta el catálogo actual de productos a formato Excel (.xlsx).
    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        var fileBytes = _excelExporter.ExportProducts(products);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Productos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    // Exporta el catálogo actual de productos a documento PDF.
    [HttpGet]
    public async Task<IActionResult> ExportPdf()
    {
        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        var fileBytes = _pdfExporter.ExportProducts(products);
        return File(fileBytes, "application/pdf", $"Productos_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}
