using System.ComponentModel.DataAnnotations;
using Firmeza.Domain.Enums;

namespace Firmeza.Application.DTOs.Products;

/// <summary>
/// Modelo de datos para crear o actualizar un producto vía API.
/// </summary>
public class SaveProductDto
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a cero")]
    public decimal Price { get; set; }

    [Range(0, 100000, ErrorMessage = "El stock debe ser cero o positivo")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [EnumDataType(typeof(ProductStatus))]
    public ProductStatus Status { get; set; } = ProductStatus.Available;

    public int? EmpresaId { get; set; }
}
