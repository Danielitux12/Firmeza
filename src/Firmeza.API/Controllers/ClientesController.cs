using AutoMapper;
using Firmeza.Application.Common;
using Firmeza.Application.DTOs.Clientes;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Firmeza.API.Problems;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Validators;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SoloAdministrador")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IExcelExporter _excelExporter;
    private readonly IPdfExporter _pdfExporter;

    public ClientesController(
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
    public async Task<ActionResult<PagedResult<ClienteDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string status = "active",
        [FromQuery] string? role = null,
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

        var baseQuery = from cliente in _context.Clientes.AsNoTracking()
                        join user in _context.Users.AsNoTracking() on cliente.UserId equals user.Id into userGroup
                        from u in userGroup.DefaultIfEmpty()
                        join userRole in _context.UserRoles.AsNoTracking() on u.Id equals userRole.UserId into urGroup
                        from ur in urGroup.DefaultIfEmpty()
                        join identityRole in _context.Roles.AsNoTracking() on ur.RoleId equals identityRole.Id into rGroup
                        from r in rGroup.DefaultIfEmpty()
                        select new {
                            Cliente = cliente,
                            RoleName = r != null ? r.Name : "Cliente"
                        };

        if (isActive.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Cliente.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            baseQuery = baseQuery.Where(x =>
                x.Cliente.Name.ToLower().Contains(term)
                || x.Cliente.Email.ToLower().Contains(term)
                || x.Cliente.DocumentNumber.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(role) && role.Trim().ToLowerInvariant() != "all")
        {
            var roleTerm = role.Trim().ToLowerInvariant();
            if (roleTerm is "admin" or "administrador")
            {
                baseQuery = baseQuery.Where(x => x.RoleName == "Administrador");
            }
            else if (roleTerm is "user" or "cliente" or "client")
            {
                baseQuery = baseQuery.Where(x => x.RoleName == "Cliente" || x.RoleName == null);
            }
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(x => x.Cliente.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(x =>
        {
            var dto = _mapper.Map<ClienteDto>(x.Cliente);
            dto.Role = x.RoleName ?? "Cliente";
            return dto;
        }).ToList();

        return Ok(new PagedResult<ClienteDto>
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await (from cliente in _context.Clientes.AsNoTracking()
                          where cliente.Id == id
                          join user in _context.Users.AsNoTracking() on cliente.UserId equals user.Id into userGroup
                          from u in userGroup.DefaultIfEmpty()
                          join userRole in _context.UserRoles.AsNoTracking() on u.Id equals userRole.UserId into urGroup
                          from ur in urGroup.DefaultIfEmpty()
                          join identityRole in _context.Roles.AsNoTracking() on ur.RoleId equals identityRole.Id into rGroup
                          from r in rGroup.DefaultIfEmpty()
                          select new {
                              Cliente = cliente,
                              RoleName = r != null ? r.Name : "Cliente"
                          }).FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return Problem(statusCode: 404, title: "Cliente no encontrado", detail: $"No existe un cliente con Id {id}.");
        }

        var dto = _mapper.Map<ClienteDto>(item.Cliente);
        dto.Role = item.RoleName ?? "Cliente";
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create(SaveClienteDto request, CancellationToken cancellationToken)
    {
        var validator = new ClienteValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Problem(statusCode: 400, title: "Datos inválidos", detail: validation.Errors[0].ErrorMessage);
        }

        var cliente = _mapper.Map<Cliente>(request);
        Normalize(cliente);

        if (await HasDuplicateAsync(cliente, null, cancellationToken))
        {
            return DuplicateProblem();
        }

        _context.Clientes.Add(cliente);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueConstraintHelper.IsUniqueViolation(exception))
        {
            return DuplicateProblem();
        }

        var dto = _mapper.Map<ClienteDto>(cliente);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClienteDto>> Update(Guid id, SaveClienteDto request, CancellationToken cancellationToken)
    {
        var validator = new ClienteValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Problem(statusCode: 400, title: "Datos inválidos", detail: validation.Errors[0].ErrorMessage);
        }

        var cliente = await _context.Clientes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (cliente is null)
        {
            return Problem(statusCode: 404, title: "Cliente no encontrado", detail: $"No existe un cliente con Id {id}.");
        }

        _mapper.Map(request, cliente);
        Normalize(cliente);

        if (await HasDuplicateAsync(cliente, id, cancellationToken))
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

        return Ok(_mapper.Map<ClienteDto>(cliente));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (cliente is null)
        {
            return Problem(statusCode: 404, title: "Cliente no encontrado", detail: $"No existe un cliente con Id {id}.");
        }

        // En lugar de borrar de la base de datos, se pasa a estado suspendido (soft-delete)
        cliente.Deactivate();

        if (!string.IsNullOrEmpty(cliente.UserId))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == cliente.UserId, cancellationToken);
            if (user != null)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/suspend")]
    public Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken) => SetActiveAsync(id, false, cancellationToken);

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (cliente is null)
        {
            return Problem(statusCode: 404, title: "Cliente no encontrado", detail: $"No existe un cliente con Id {id}.");
        }

        cliente.Activate();

        if (!string.IsNullOrEmpty(cliente.UserId))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == cliente.UserId, cancellationToken);
            if (user != null)
            {
                user.LockoutEnd = null;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(_mapper.Map<ClienteDto>(cliente));
    }

    [HttpDelete("{id:guid}/permanent")]
    public async Task<IActionResult> DeletePermanent(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (cliente is null)
        {
            return Problem(statusCode: 404, title: "Cliente no encontrado", detail: $"No existe un cliente con Id {id}.");
        }

        if (!string.IsNullOrEmpty(cliente.UserId))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == cliente.UserId, cancellationToken);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<IActionResult> SetActiveAsync(Guid id, bool active, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (cliente is null)
        {
            return Problem(statusCode: 404, title: "Cliente no encontrado", detail: $"No existe un cliente con Id {id}.");
        }

        if (active) cliente.Activate(); else cliente.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(_mapper.Map<ClienteDto>(cliente));
    }

    private Task<bool> HasDuplicateAsync(Cliente cliente, Guid? excludingId, CancellationToken cancellationToken)
    {
        var normalizedEmail = cliente.Email.Trim().ToLower();
        var normalizedDocument = cliente.DocumentNumber.Trim().ToLower();
        var query = _context.Clientes.AsQueryable();
        if (excludingId.HasValue)
        {
            query = query.Where(item => item.Id != excludingId.Value);
        }

        return query.AnyAsync(item => item.Email.ToLower() == normalizedEmail
            || item.DocumentNumber.ToLower() == normalizedDocument, cancellationToken);
    }

    private static void Normalize(Cliente cliente)
    {
        cliente.Name = cliente.Name.Trim();
        cliente.DocumentNumber = cliente.DocumentNumber.Trim().ToUpperInvariant();
        cliente.Email = cliente.Email.Trim().ToLowerInvariant();
        cliente.Phone = cliente.Phone.Trim();
        cliente.Address = cliente.Address.Trim();
    }

    private ObjectResult DuplicateProblem() => Problem(
        statusCode: 409,
        title: "Conflicto de datos",
        detail: "El correo electrónico o el número de documento ya pertenece a otro cliente.");

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
        var clientes = await (from c in _context.Clientes.AsNoTracking()
                              where c.IsActive
                              join u in _context.Users.AsNoTracking() on c.UserId equals u.Id into uGroup
                              from user in uGroup.DefaultIfEmpty()
                              join ur in _context.UserRoles.AsNoTracking() on user.Id equals ur.UserId into urGroup
                              from userRole in urGroup.DefaultIfEmpty()
                              join r in _context.Roles.AsNoTracking() on userRole.RoleId equals r.Id into rGroup
                              from role in rGroup.DefaultIfEmpty()
                              orderby c.Name
                              select new ClienteDto
                              {
                                  Id = c.Id,
                                  Name = c.Name,
                                  DocumentNumber = c.DocumentNumber,
                                  Email = c.Email,
                                  Phone = c.Phone,
                                  Address = c.Address,
                                  IsActive = c.IsActive,
                                  Role = (role != null && role.Name != null) ? role.Name : "Cliente",
                                  Age = c.BirthDate.HasValue ? DateTime.UtcNow.Year - c.BirthDate.Value.Year : 0
                              }).ToListAsync(cancellationToken);

        var fileBytes = _excelExporter.ExportClientes(clientes);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Clientes_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet("export-pdf")]
    public async Task<IActionResult> ExportPdf(CancellationToken cancellationToken = default)
    {
        var clientes = await (from c in _context.Clientes.AsNoTracking()
                              where c.IsActive
                              join u in _context.Users.AsNoTracking() on c.UserId equals u.Id into uGroup
                              from user in uGroup.DefaultIfEmpty()
                              join ur in _context.UserRoles.AsNoTracking() on user.Id equals ur.UserId into urGroup
                              from userRole in urGroup.DefaultIfEmpty()
                              join r in _context.Roles.AsNoTracking() on userRole.RoleId equals r.Id into rGroup
                              from role in rGroup.DefaultIfEmpty()
                              orderby c.Name
                              select new ClienteDto
                              {
                                  Id = c.Id,
                                  Name = c.Name,
                                  DocumentNumber = c.DocumentNumber,
                                  Email = c.Email,
                                  Phone = c.Phone,
                                  Address = c.Address,
                                  IsActive = c.IsActive,
                                  Role = (role != null && role.Name != null) ? role.Name : "Cliente",
                                  Age = c.BirthDate.HasValue ? DateTime.UtcNow.Year - c.BirthDate.Value.Year : 0
                              }).ToListAsync(cancellationToken);

        var fileBytes = _pdfExporter.ExportClientes(clientes);
        return File(fileBytes, "application/pdf", $"Clientes_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }
}