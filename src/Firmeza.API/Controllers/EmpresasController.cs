using AutoMapper;
using Firmeza.API.Problems;
using Firmeza.Application.Common;
using Firmeza.Application.DTOs.Empresas;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Firmeza.Application.Validators;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SoloAdministrador")]
public class EmpresasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExcelExporter _excelExporter;
    private readonly IPdfExporter _pdfExporter;

    public EmpresasController(
        AppDbContext context,
        IMapper mapper,
        IExcelExporter excelExporter,
        IPdfExporter pdfExporter)
    {
        _context = context;
        _mapper = mapper;
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EmpresaDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string status = "active",
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1)
        {
            return Problem(statusCode: 400, title: "Solicitud inválida", detail: "La página y el tamaño de página deben ser mayores que cero.");
        }

        pageSize = Math.Min(pageSize, 100);
        if (!TryGetActiveFilter(status, out var isActive))
        {
            return Problem(statusCode: 400, title: "Estado inválido", detail: "El estado debe ser active, inactive o all.");
        }

        var query = _context.Empresas.AsNoTracking();
        if (isActive.HasValue)
        {
            query = query.Where(empresa => empresa.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(empresa =>
                empresa.Name.ToLower().Contains(term)
                || empresa.Email.ToLower().Contains(term)
                || empresa.Nit.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(empresa => empresa.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<EmpresaDto>
        {
            Items = _mapper.Map<List<EmpresaDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmpresaDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var empresa = await _context.Empresas.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return empresa is null
            ? Problem(statusCode: 404, title: "Empresa no encontrada", detail: $"No existe una empresa con Id {id}.")
            : Ok(_mapper.Map<EmpresaDto>(empresa));
    }

    [HttpPost]
    public async Task<ActionResult<EmpresaDto>> Create(SaveEmpresaDto request, CancellationToken cancellationToken)
    {
        var validator = new EmpresaValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Problem(statusCode: 400, title: "Datos inválidos", detail: validation.Errors[0].ErrorMessage);
        }

        var empresa = _mapper.Map<Empresa>(request);
        Normalize(empresa);

        if (await HasDuplicateAsync(empresa, null, cancellationToken))
        {
            return DuplicateProblem();
        }

        _context.Empresas.Add(empresa);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueConstraintHelper.IsUniqueViolation(exception))
        {
            return DuplicateProblem();
        }

        return CreatedAtAction(nameof(GetById), new { id = empresa.Id }, _mapper.Map<EmpresaDto>(empresa));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmpresaDto>> Update(Guid id, SaveEmpresaDto request, CancellationToken cancellationToken)
    {
        var validator = new EmpresaValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Problem(statusCode: 400, title: "Datos inválidos", detail: validation.Errors[0].ErrorMessage);
        }

        var empresa = await _context.Empresas.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (empresa is null)
        {
            return Problem(statusCode: 404, title: "Empresa no encontrada", detail: $"No existe una empresa con Id {id}.");
        }

        _mapper.Map(request, empresa);
        Normalize(empresa);

        if (await HasDuplicateAsync(empresa, id, cancellationToken))
        {
            return DuplicateProblem();
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueConstraintHelper.IsUniqueViolation(exception))
        {
            return DuplicateProblem();
        }

        return Ok(_mapper.Map<EmpresaDto>(empresa));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (empresa is null)
        {
            return Problem(statusCode: 404, title: "Empresa no encontrada", detail: $"No existe una empresa con Id {id}.");
        }

        // Baja lógica: pasar a estado suspendido en vez de eliminar registro físico
        empresa.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/suspend")]
    public Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken) => SetActiveAsync(id, false, cancellationToken);

    [HttpPatch("{id:guid}/activate")]
    public Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken) => SetActiveAsync(id, true, cancellationToken);

    private async Task<IActionResult> SetActiveAsync(Guid id, bool active, CancellationToken cancellationToken)
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (empresa is null)
        {
            return Problem(statusCode: 404, title: "Empresa no encontrada", detail: $"No existe una empresa con Id {id}.");
        }

        if (active) empresa.Activate(); else empresa.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(_mapper.Map<EmpresaDto>(empresa));
    }

    private Task<bool> HasDuplicateAsync(Empresa empresa, Guid? excludingId, CancellationToken cancellationToken)
    {
        var normalizedEmail = empresa.Email.Trim().ToLower();
        var normalizedNit = empresa.Nit.Trim().ToLower();
        var query = _context.Empresas.AsQueryable();
        if (excludingId.HasValue)
        {
            query = query.Where(item => item.Id != excludingId.Value);
        }

        return query.AnyAsync(item => item.Email.ToLower() == normalizedEmail
            || item.Nit.ToLower() == normalizedNit, cancellationToken);
    }

    private static void Normalize(Empresa empresa)
    {
        empresa.Name = empresa.Name.Trim();
        empresa.Nit = empresa.Nit.Trim().ToUpperInvariant();
        empresa.Email = empresa.Email.Trim().ToLowerInvariant();
        empresa.Phone = empresa.Phone.Trim();
        empresa.Address = empresa.Address.Trim();
    }

    private ObjectResult DuplicateProblem() => Problem(
        statusCode: 409,
        title: "Conflicto de datos",
        detail: "El correo electrónico o el NIT ya pertenece a otra empresa.");

    private static bool TryGetActiveFilter(string status, out bool? isActive)
    {
        switch (status.Trim().ToLowerInvariant())
        {
            case "active": isActive = true; return true;
            case "inactive": isActive = false; return true;
            case "all": isActive = null; return true;
            default: isActive = null; return false;
        }
    }

    [HttpGet("export-excel")]
    public async Task<IActionResult> ExportExcel(CancellationToken cancellationToken = default)
    {
        var empresas = await _context.Empresas
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .Select(e => new EmpresaDto
            {
                Id = e.Id,
                Name = e.Name,
                Nit = e.Nit,
                Email = e.Email,
                Phone = e.Phone,
                Address = e.Address,
                IsActive = e.IsActive
            })
            .ToListAsync(cancellationToken);

        var fileBytes = _excelExporter.ExportEmpresas(empresas);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Empresas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet("export-pdf")]
    public async Task<IActionResult> ExportPdf(CancellationToken cancellationToken = default)
    {
        var empresas = await _context.Empresas
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .Select(e => new EmpresaDto
            {
                Id = e.Id,
                Name = e.Name,
                Nit = e.Nit,
                Email = e.Email,
                Phone = e.Phone,
                Address = e.Address,
                IsActive = e.IsActive
            })
            .ToListAsync(cancellationToken);

        var fileBytes = _pdfExporter.ExportEmpresas(empresas);
        return File(fileBytes, "application/pdf", $"Empresas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}