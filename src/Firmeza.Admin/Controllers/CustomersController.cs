using Firmeza.Application.Interfaces;
using Firmeza.Application.ViewModels.Customers;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Admin.Controllers;

/// <summary>
/// Gestiona el registro y consulta de clientes con validaciones y manejo de excepciones en edad.
/// </summary>
[Authorize(Roles = "Administrador")]
public class CustomersController : Controller
{
    private readonly AppDbContext _context;
    private readonly IExcelExporter _excelExporter;
    private readonly IPdfExporter _pdfExporter;

    public CustomersController(
        AppDbContext context,
        IExcelExporter excelExporter,
        IPdfExporter pdfExporter)
    {
        _context = context;
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
    }

    // Muestra la lista de clientes con soporte para búsqueda por nombre o número de documento.
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c => c.FullName.ToLower().Contains(term) || c.DocumentNumber.ToLower().Contains(term));
        }

        var customers = await query
            .OrderBy(c => c.FullName)
            .Select(c => new CustomerViewModel
            {
                Id = c.Id,
                FullName = c.FullName,
                DocumentNumber = c.DocumentNumber,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                // Calcula la edad si la fecha de nacimiento está registrada.
                ProcessedAge = c.BirthDate.HasValue ? DateTime.UtcNow.Year - c.BirthDate.Value.Year : null
            })
            .ToListAsync();

        var model = new CustomerFilterViewModel
        {
            SearchTerm = searchTerm,
            Customers = customers
        };

        return View(model);
    }

    // Muestra el formulario para crear un nuevo cliente.
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CustomerViewModel());
    }

    // Procesa la creación del cliente, validando unicidad de correo/documento y convirtiendo la edad con try-catch.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerViewModel model)
    {
        // Requisito 6: El campo "Edad" se recibe como texto; convertir con int.Parse dentro de try-catch y mostrar mensaje amigable.
        int parsedAge = 0;
        try
        {
            if (string.IsNullOrWhiteSpace(model.AgeText))
            {
                ModelState.AddModelError(nameof(model.AgeText), "Por favor ingresa la edad del cliente.");
            }
            else
            {
                parsedAge = int.Parse(model.AgeText.Trim());
                if (parsedAge < 0 || parsedAge > 125)
                {
                    ModelState.AddModelError(nameof(model.AgeText), "La edad debe encontrarse entre 0 y 125 años.");
                }
            }
        }
        catch (FormatException)
        {
            // Mensaje amigable cuando el texto no es un entero válido.
            ModelState.AddModelError(nameof(model.AgeText), "La edad debe ser un número entero válido (por ejemplo, 30). No incluyas letras, signos ni decimales.");
        }
        catch (OverflowException)
        {
            ModelState.AddModelError(nameof(model.AgeText), "El número de edad ingresado excede el límite permitido.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(nameof(model.AgeText), $"Ocurrió un error inesperado al procesar la edad: {ex.Message}");
        }

        // Valida que el documento y el correo no existan ya registrados.
        if (await _context.Customers.AnyAsync(c => c.DocumentNumber == model.DocumentNumber.Trim()))
        {
            ModelState.AddModelError(nameof(model.DocumentNumber), "Ya existe un cliente registrado con este número de documento.");
        }

        if (await _context.Customers.AnyAsync(c => c.Email == model.Email.Trim().ToLower()))
        {
            ModelState.AddModelError(nameof(model.Email), "Ya existe un cliente registrado con este correo electrónico.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = new Customer
        {
            FullName = model.FullName.Trim(),
            DocumentNumber = model.DocumentNumber.Trim(),
            Email = model.Email.Trim().ToLower(),
            Phone = model.Phone.Trim(),
            Address = model.Address.Trim(),
            // Guarda la fecha de nacimiento aproximada a partir de la edad ingresada.
            BirthDate = DateTime.UtcNow.AddYears(-parsedAge)
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"El cliente '{customer.FullName}' fue registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Muestra el formulario para editar los datos de un cliente existente.
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        int? currentAge = customer.BirthDate.HasValue ? DateTime.UtcNow.Year - customer.BirthDate.Value.Year : null;

        var model = new CustomerViewModel
        {
            Id = customer.Id,
            FullName = customer.FullName,
            DocumentNumber = customer.DocumentNumber,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            AgeText = currentAge?.ToString() ?? string.Empty,
            ProcessedAge = currentAge
        };

        return View(model);
    }

    // Guarda las modificaciones realizadas sobre un cliente.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        // Requisito 6: Conversión de Edad con try-catch.
        int parsedAge = 0;
        try
        {
            if (string.IsNullOrWhiteSpace(model.AgeText))
            {
                ModelState.AddModelError(nameof(model.AgeText), "Por favor ingresa la edad del cliente.");
            }
            else
            {
                parsedAge = int.Parse(model.AgeText.Trim());
                if (parsedAge < 0 || parsedAge > 125)
                {
                    ModelState.AddModelError(nameof(model.AgeText), "La edad debe encontrarse entre 0 y 125 años.");
                }
            }
        }
        catch (FormatException)
        {
            ModelState.AddModelError(nameof(model.AgeText), "La edad debe ser un número entero válido (por ejemplo, 30). No incluyas letras, signos ni decimales.");
        }
        catch (OverflowException)
        {
            ModelState.AddModelError(nameof(model.AgeText), "El número de edad ingresado excede el límite permitido.");
        }

        // Valida unicidad excluyendo al cliente actual.
        if (await _context.Customers.AnyAsync(c => c.DocumentNumber == model.DocumentNumber.Trim() && c.Id != id))
        {
            ModelState.AddModelError(nameof(model.DocumentNumber), "Ya existe otro cliente con este número de documento.");
        }

        if (await _context.Customers.AnyAsync(c => c.Email == model.Email.Trim().ToLower() && c.Id != id))
        {
            ModelState.AddModelError(nameof(model.Email), "Ya existe otro cliente con este correo electrónico.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        customer.FullName = model.FullName.Trim();
        customer.DocumentNumber = model.DocumentNumber.Trim();
        customer.Email = model.Email.Trim().ToLower();
        customer.Phone = model.Phone.Trim();
        customer.Address = model.Address.Trim();
        customer.BirthDate = DateTime.UtcNow.AddYears(-parsedAge);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Los datos de '{customer.FullName}' se actualizaron correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Muestra la vista detallada de un cliente.
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Sales)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return NotFound();
        }

        var model = new CustomerViewModel
        {
            Id = customer.Id,
            FullName = customer.FullName,
            DocumentNumber = customer.DocumentNumber,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            ProcessedAge = customer.BirthDate.HasValue ? DateTime.UtcNow.Year - customer.BirthDate.Value.Year : null
        };

        return View(model);
    }

    // Muestra la pantalla de confirmación para eliminar un cliente.
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return NotFound();
        }

        var model = new CustomerViewModel
        {
            Id = customer.Id,
            FullName = customer.FullName,
            DocumentNumber = customer.DocumentNumber,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            ProcessedAge = customer.BirthDate.HasValue ? DateTime.UtcNow.Year - customer.BirthDate.Value.Year : null
        };

        return View(model);
    }

    // Procesa la eliminación del cliente si no posee ventas asociadas.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        // Verifica si tiene ventas registradas para no romper la integridad referencial.
        var hasSales = await _context.Sales.AnyAsync(s => s.CustomerId == id);
        if (hasSales)
        {
            TempData["ErrorMessage"] = $"No se puede eliminar el cliente '{customer.FullName}' porque tiene ventas asociadas en el sistema.";
            return RedirectToAction(nameof(Index));
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"El cliente '{customer.FullName}' fue eliminado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // Exporta el directorio de clientes a formato Excel (.xlsx).
    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .ToListAsync();

        var fileBytes = _excelExporter.ExportCustomers(customers);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Clientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    // Exporta el directorio de clientes a documento PDF.
    [HttpGet]
    public async Task<IActionResult> ExportPdf()
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .ToListAsync();

        var fileBytes = _pdfExporter.ExportCustomers(customers);
        return File(fileBytes, "application/pdf", $"Clientes_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}
