using Firmeza.Application.Interfaces;
using Firmeza.Application.ViewModels.Clientes;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Firmeza.Admin.Controllers;

/// <summary>
/// Gestiona el registro y consulta de clientes con validaciones y manejo de excepciones en edad.
/// </summary>
[Authorize(Roles = "Administrador")]
public class ClientesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IExcelExporter _excelExporter;
    private readonly IPdfExporter _pdfExporter;

    public ClientesController(
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
    public async Task<IActionResult> Index(string? search, string status = "active", int page = 1, int pageSize = 10)
    {
        var query = _context.Clientes.AsNoTracking().AsQueryable();
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        status = status.Trim().ToLowerInvariant();
        if (status is not ("active" or "inactive" or "all"))
        {
            status = "active";
        }

        if (status != "all")
        {
            var active = status == "active";
            query = query.Where(c => c.IsActive == active);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term)
                || c.Email.ToLower().Contains(term)
                || c.DocumentNumber.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var clientes = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClienteViewModel
            {
                Id = c.Id,
                IsActive = c.IsActive,
                Name = c.Name,
                DocumentNumber = c.DocumentNumber,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                // Calcula la edad si la fecha de nacimiento está registrada.
                ProcessedAge = c.BirthDate.HasValue ? DateTime.UtcNow.Year - c.BirthDate.Value.Year : null
            })
            .ToListAsync();

        var model = new ClienteFilterViewModel
        {
            SearchTerm = search,
            Status = status,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Clientes = clientes,
        };

        return View(model);
    }

    // Muestra el formulario para crear un nuevo cliente.
    [HttpGet]
    public IActionResult Create()
    {
        return View(new ClienteViewModel());
    }

    // Procesa la creación del cliente, validando unicidad de correo/documento y convirtiendo la edad con try-catch.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteViewModel model)
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

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Valida que el documento y el correo no existan ya registrados.
        var normalizedDocument = model.DocumentNumber.Trim().ToUpperInvariant();
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        if (await _context.Clientes.AnyAsync(c => c.DocumentNumber.ToUpper() == normalizedDocument))
        {
            ModelState.AddModelError(nameof(model.DocumentNumber), "Ya existe un cliente registrado con este número de documento.");
        }

        if (await _context.Clientes.AnyAsync(c => c.Email.ToLower() == normalizedEmail))
        {
            ModelState.AddModelError(nameof(model.Email), "Ya existe un cliente registrado con este correo electrónico.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cliente = new Cliente
        {
            Name = model.Name.Trim(),
            DocumentNumber = normalizedDocument,
            Email = normalizedEmail,
            Phone = model.Phone.Trim(),
            Address = model.Address.Trim(),
            // Guarda la fecha de nacimiento aproximada a partir de la edad ingresada.
            BirthDate = DateTime.UtcNow.AddYears(-parsedAge)
        };

        if (!cliente.IsValid())
        {
            ModelState.AddModelError(string.Empty, "Los datos del cliente no son válidos.");
            return View(model);
        }

        _context.Clientes.Add(cliente);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "El correo o documento ya pertenece a otro cliente.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"El cliente '{cliente.Name}' fue registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Muestra el formulario para editar los datos de un cliente existente.
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound();
        }

        int? currentAge = cliente.BirthDate.HasValue ? DateTime.UtcNow.Year - cliente.BirthDate.Value.Year : null;

        var model = new ClienteViewModel
        {
            Id = cliente.Id,
            IsActive = cliente.IsActive,
            Name = cliente.Name,
            DocumentNumber = cliente.DocumentNumber,
            Email = cliente.Email,
            Phone = cliente.Phone,
            Address = cliente.Address,
            AgeText = currentAge?.ToString() ?? string.Empty,
            ProcessedAge = currentAge
        };

        return View(model);
    }

    // Guarda las modificaciones realizadas sobre un cliente.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteViewModel model)
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

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Valida unicidad excluyendo al cliente actual.
        var normalizedDocument = model.DocumentNumber.Trim().ToUpperInvariant();
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        if (await _context.Clientes.AnyAsync(c => c.DocumentNumber.ToUpper() == normalizedDocument && c.Id != id))
        {
            ModelState.AddModelError(nameof(model.DocumentNumber), "Ya existe otro cliente con este número de documento.");
        }

        if (await _context.Clientes.AnyAsync(c => c.Email.ToLower() == normalizedEmail && c.Id != id))
        {
            ModelState.AddModelError(nameof(model.Email), "Ya existe otro cliente con este correo electrónico.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound();
        }

        cliente.Name = model.Name.Trim();
        cliente.DocumentNumber = normalizedDocument;
        cliente.Email = normalizedEmail;
        cliente.Phone = model.Phone.Trim();
        cliente.Address = model.Address.Trim();
        cliente.BirthDate = DateTime.UtcNow.AddYears(-parsedAge);

        if (!cliente.IsValid())
        {
            ModelState.AddModelError(string.Empty, "Los datos del cliente no son válidos.");
            return View(model);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "El correo o documento ya pertenece a otro cliente.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"Los datos de '{cliente.Name}' se actualizaron correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Muestra la vista detallada de un cliente.
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _context.Clientes
            .AsNoTracking()
            .Include(c => c.Sales)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente is null)
        {
            return NotFound();
        }

        var model = new ClienteViewModel
        {
            Id = cliente.Id,
            IsActive = cliente.IsActive,
            Name = cliente.Name,
            DocumentNumber = cliente.DocumentNumber,
            Email = cliente.Email,
            Phone = cliente.Phone,
            Address = cliente.Address,
            ProcessedAge = cliente.BirthDate.HasValue ? DateTime.UtcNow.Year - cliente.BirthDate.Value.Year : null
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound();
        }

        cliente.Deactivate();
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"El cliente '{cliente.Name}' fue suspendido.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound();
        }

        cliente.Activate();
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"El cliente '{cliente.Name}' fue activado.";
        return RedirectToAction(nameof(Index));
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    // Exporta el directorio de clientes a formato Excel (.xlsx).
    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var clientes = await _context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        var fileBytes = _excelExporter.ExportClientes(clientes);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Clientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    // Exporta el directorio de clientes a documento PDF.
    [HttpGet]
    public async Task<IActionResult> ExportPdf()
    {
        var clientes = await _context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        var fileBytes = _pdfExporter.ExportClientes(clientes);
        return File(fileBytes, "application/pdf", $"Clientes_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}
