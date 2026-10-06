using AutoMapper;
using Firmeza.Application.DTOs.Customers;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Controlador REST para gestión de clientes.
/// Toda la administración de clientes está restringida a administradores ('SoloAdministrador').
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SoloAdministrador")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CustomersController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista todos los clientes registrados.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        var customers = await _context.Customers.AsNoTracking().ToListAsync();
        return Ok(_mapper.Map<List<CustomerDto>>(customers));
    }

    /// <summary>
    /// Obtiene un cliente por su identificador.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound(new { message = $"Cliente con id {id} no encontrado." });
        }

        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    /// <summary>
    /// Crea un cliente nuevo desde el API.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] SaveCustomerDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var exists = await _context.Customers.AnyAsync(c => c.DocumentNumber == request.DocumentNumber || c.Email == request.Email);
        if (exists)
        {
            return BadRequest(new { message = "Ya existe un cliente con ese documento o correo." });
        }

        var customer = _mapper.Map<Customer>(request);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var dto = _mapper.Map<CustomerDto>(customer);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, dto);
    }

    /// <summary>
    /// Actualiza la información de un cliente existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveCustomerDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound(new { message = $"Cliente con id {id} no encontrado." });
        }

        var duplicate = await _context.Customers.AnyAsync(c => c.Id != id && (c.DocumentNumber == request.DocumentNumber || c.Email == request.Email));
        if (duplicate)
        {
            return BadRequest(new { message = "Ya existe otro cliente con ese documento o correo." });
        }

        _mapper.Map(request, customer);
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<CustomerDto>(customer));
    }

    /// <summary>
    /// Elimina un cliente si no tiene ventas asociadas.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.Include(c => c.Sales).FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null)
        {
            return NotFound(new { message = $"Cliente con id {id} no encontrado." });
        }

        if (customer.Sales.Any())
        {
            return BadRequest(new { message = "No se puede eliminar el cliente porque tiene ventas registradas." });
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
