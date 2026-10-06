using System.ComponentModel.DataAnnotations;
using Firmeza.Domain.Enums;

namespace Firmeza.Application.ViewModels.Products;

/// <summary>
/// Modelo de vista para la creación, edición y visualización de productos.
/// </summary>
public class ProductViewModel
{
    // Identificador único del producto (0 al crear).
    public int Id { get; set; }

    // Nombre comercial del producto.
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    // Descripción del producto.
    [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres.")]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    // Precio unitario.
    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 1000000.00, ErrorMessage = "El precio debe ser mayor a 0.")]
    [Display(Name = "Precio ($)")]
    public decimal Price { get; set; }

    // Cantidad en inventario.
    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 100000, ErrorMessage = "El stock debe ser 0 o superior.")]
    [Display(Name = "Stock Disponible")]
    public int Stock { get; set; }

    // Categoría del producto.
    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(100, ErrorMessage = "La categoría no puede exceder 100 caracteres.")]
    [Display(Name = "Categoría")]
    public string Category { get; set; } = string.Empty;

    [EnumDataType(typeof(ProductStatus))]
    [Display(Name = "Estado")]
    public ProductStatus Status { get; set; } = ProductStatus.Available;

    public string StatusLabel => Status switch
    {
        ProductStatus.Available => "Disponible",
        ProductStatus.Unavailable => "No disponible",
        ProductStatus.Discontinued => "Descontinuado",
        _ => "Desconocido"
    };
}
