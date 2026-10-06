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
        await ValidateDuplicatesAsync(model, null);
        var empresa = ToEntity(model);
        if (!empresa.IsValid()) ModelState.AddModelError(string.Empty, "Los datos de la empresa no son válidos.");
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
    public async Task<IActionResult> Edit(int id)
    {
        var empresa = await _context.Empresas.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return empresa is null ? NotFound() : View(ToViewModel(empresa));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmpresaViewModel model)
    {
        if (id != model.Id) return BadRequest();

        var empresa = await _context.Empresas.FirstOrDefaultAsync(item => item.Id == id);
        if (empresa is null) return NotFound();

        await ValidateDuplicatesAsync(model, id);
        var candidate = ToEntity(model);
        if (!candidate.IsValid()) ModelState.AddModelError(string.Empty, "Los datos de la empresa no son válidos.");
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
    public Task<IActionResult> Suspend(int id) => SetActiveAsync(id, false);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Activate(int id) => SetActiveAsync(id, true);

    private async Task<IActionResult> SetActiveAsync(int id, bool active)
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync(item => item.Id == id);
        if (empresa is null) return NotFound();

        if (active) empresa.Activate(); else empresa.Deactivate();
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = active ? "Empresa activada." : "Empresa suspendida.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateDuplicatesAsync(EmpresaViewModel model, int? excludingId)
    {
        var email = model.Email.Trim().ToLower();
        var nit = model.Nit.Trim().ToLower();
        if (await _context.Empresas.AnyAsync(item => item.Id != excludingId && item.Email.ToLower() == email))
            ModelState.AddModelError(nameof(model.Email), "El correo ya pertenece a otra empresa.");
        if (await _context.Empresas.AnyAsync(item => item.Id != excludingId && item.Nit.ToLower() == nit))
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