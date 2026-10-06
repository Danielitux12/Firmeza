using AutoMapper;
using Firmeza.Application.DTOs.Products;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Controlador REST para productos.
/// Permite listar y consultar a cualquier usuario autenticado (incluyendo clientes),
/// pero limita la creación, edición y eliminación exclusivamente a 'SoloAdministrador'.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProductsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Lista todos los productos disponibles. Accesible tanto para Clientes como Administradores.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _context.Products.AsNoTracking().ToListAsync();
        var dtos = _mapper.Map<List<ProductDto>>(products);
        return Ok(dtos);
    }

    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Producto con id {id} no encontrado." });
        }

        return Ok(_mapper.Map<ProductDto>(product));
    }

    /// <summary>
    /// Crea un nuevo producto en el catálogo. Exclusivo para Administrador.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "SoloAdministrador")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] SaveProductDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = _mapper.Map<Product>(request);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var dto = _mapper.Map<ProductDto>(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, dto);
    }

    /// <summary>
    /// Actualiza un producto existente. Exclusivo para Administrador.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "SoloAdministrador")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveProductDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Producto con id {id} no encontrado." });
        }

        // Mapeamos los campos actualizados sobre la entidad existente
        _mapper.Map(request, product);
        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<ProductDto>(product));
    }

    /// <summary>
    /// Elimina un producto. Exclusivo para Administrador.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "SoloAdministrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Producto con id {id} no encontrado." });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
