using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Sales;

/// <summary>
/// Modelo de ítem de producto dentro de una solicitud de creación de venta.
/// </summary>
public class CreateSaleItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El Id de producto es inválido")]
    public int ProductId { get; set; }

    [Range(1, 10000, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; set; }
}
