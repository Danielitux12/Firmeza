using Firmeza.Domain.Enums;

namespace Firmeza.Application.DTOs.Products;

/// <summary>
/// Modelo de datos para consultar o responder información de un producto.
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
    public int? EmpresaId { get; set; }
    public bool IsActive { get; set; }
}
