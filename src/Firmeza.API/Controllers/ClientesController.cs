using AutoMapper;
using Firmeza.Application.Common;
using Firmeza.Application.DTOs.Clientes;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Firmeza.API.Problems;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SoloAdministrador")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ClientesController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClienteDto>>> GetAll(
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

        var query = _context.Clientes.AsNoTracking();
        if (isActive.HasValue)
        {
            query = query.Where(cliente => cliente.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(cliente =>
                cliente.Name.ToLower().Contains(term)
                || cliente.Email.ToLower().Contains(term)
                || cliente.DocumentNumber.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(cliente => cliente.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<ClienteDto>
        {
            Items = _mapper.Map<List<ClienteDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return cliente is null
            ? Problem(statusCode: 404, title: "Cliente no encontrado", detail: $"No existe un cliente con Id {id}.")
            : Ok(_mapper.Map<ClienteDto>(cliente));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create(SaveClienteDto request, CancellationToken cancellationToken)
    {
        var cliente = _mapper.Map<Cliente>(request);
        if (!cliente.IsValid())
        {
            return Problem(statusCode: 400, title: "Datos inválidos", detail: "El nombre, documento y correo del cliente deben ser válidos.");
        }

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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Update(int id, SaveClienteDto request, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (cliente is null)
        {
            return Problem(statusCode: 404, title: "Cliente no encontrado", detail: $"No existe un cliente con Id {id}.");
        }

        _mapper.Map(request, cliente);
        if (!cliente.IsValid())
        {
            return Problem(statusCode: 400, title: "Datos inválidos", detail: "El nombre, documento y correo del cliente deben ser válidos.");
        }

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

    [HttpPatch("{id:int}/suspend")]
    public Task<IActionResult> Suspend(int id, CancellationToken cancellationToken) => SetActiveAsync(id, false, cancellationToken);

    [HttpPatch("{id:int}/activate")]
    public Task<IActionResult> Activate(int id, CancellationToken cancellationToken) => SetActiveAsync(id, true, cancellationToken);

    private async Task<IActionResult> SetActiveAsync(int id, bool active, CancellationToken cancellationToken)
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

    private Task<bool> HasDuplicateAsync(Cliente cliente, int? excludingId, CancellationToken cancellationToken)
    {
        var normalizedEmail = cliente.Email.Trim().ToLower();
        var normalizedDocument = cliente.DocumentNumber.Trim().ToLower();
        return _context.Clientes.AnyAsync(item => item.Id != excludingId
            && (item.Email.ToLower() == normalizedEmail || item.DocumentNumber.ToLower() == normalizedDocument), cancellationToken);
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
}