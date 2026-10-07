using Firmeza.Application.ViewModels.Empresas;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Firmeza.Admin.Controllers;

[Authorize(Roles = "Administrador")]
public class EmpresasController : Controller
{
    private readonly AppDbContext _context;

    public EmpresasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string status = "active", int page = 1, int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        status = status.Trim().ToLowerInvariant();
        if (status is not ("active" or "inactive" or "all")) status = "active";

        var query = _context.Empresas.AsNoTracking().AsQueryable();
        if (status != "all")
        {
            var active = status == "active";
            query = query.Where(empresa => empresa.IsActive == active);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(empresa => empresa.Name.ToLower().Contains(term)
                || empresa.Email.ToLower().Contains(term)
                || empresa.Nit.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(empresa => empresa.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(empresa => new EmpresaViewModel
            {
                Id = empresa.Id,
                Name = empresa.Name,
                Nit = empresa.Nit,
                Email = empresa.Email,
                Phone = empresa.Phone,
                Address = empresa.Address,
                IsActive = empresa.IsActive
            })
            .ToListAsync();

        return View(new EmpresaFilterViewModel
        {
            Search = search,
            Status = status,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        });
    }

    [HttpGet]
    public IActionResult Create() => View(new EmpresaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmpresaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        await ValidateDuplicatesAsync(model, null);
        var empresa = ToEntity(model);
        if (!ModelState.IsValid) return View(model);

        _context.Empresas.Add(empresa);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "El correo o NIT ya pertenece a otra empresa.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"La empresa '{empresa.Name}' fue registrada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var empresa = await _context.Empresas.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return empresa is null ? NotFound() : View(ToViewModel(empresa));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EmpresaViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var empresa = await _context.Empresas.FirstOrDefaultAsync(item => item.Id == id);
        if (empresa is null) return NotFound();

        await ValidateDuplicatesAsync(model, id);
        var candidate = ToEntity(model);
        if (!ModelState.IsValid) return View(model);

        empresa.Name = candidate.Name;
        empresa.Nit = candidate.Nit;
        empresa.Email = candidate.Email;
        empresa.Phone = candidate.Phone;
        empresa.Address = candidate.Address;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(string.Empty, "El correo o NIT ya pertenece a otra empresa.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"La empresa '{empresa.Name}' fue actualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Suspend(Guid id) => SetActiveAsync(id, false);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Activate(Guid id) => SetActiveAsync(id, true);

    private async Task<IActionResult> SetActiveAsync(Guid id, bool active)
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync(item => item.Id == id);
        if (empresa is null) return NotFound();

        if (active) empresa.Activate(); else empresa.Deactivate();
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = active ? "Empresa activada." : "Empresa suspendida.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateDuplicatesAsync(EmpresaViewModel model, Guid? excludingId)
    {
        var email = model.Email.Trim().ToLower();
        var nit = model.Nit.Trim().ToLower();
        var query = _context.Empresas.AsQueryable();
        if (excludingId.HasValue) query = query.Where(item => item.Id != excludingId.Value);
        if (await query.AnyAsync(item => item.Email.ToLower() == email))
            ModelState.AddModelError(nameof(model.Email), "El correo ya pertenece a otra empresa.");
        if (await query.AnyAsync(item => item.Nit.ToLower() == nit))
            ModelState.AddModelError(nameof(model.Nit), "El NIT ya pertenece a otra empresa.");
    }

    private static Empresa ToEntity(EmpresaViewModel model) => new()
    {
        Name = model.Name.Trim(),
        Nit = model.Nit.Trim(),
        Email = model.Email.Trim().ToLowerInvariant(),
        Phone = model.Phone.Trim(),
        Address = model.Address.Trim()
    };

    private static EmpresaViewModel ToViewModel(Empresa empresa) => new()
    {
        Id = empresa.Id,
        Name = empresa.Name,
        Nit = empresa.Nit,
        Email = empresa.Email,
        Phone = empresa.Phone,
        Address = empresa.Address,
        IsActive = empresa.IsActive
    };

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}