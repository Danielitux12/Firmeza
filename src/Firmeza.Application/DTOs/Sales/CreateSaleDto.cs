using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Sales;

/// <summary>
/// Modelo de solicitud para registrar una nueva venta vía API.
/// Si es enviada por un Administrador, puede especificar ClienteId.
/// Si es enviada por un Cliente, el sistema asigna automáticamente su ClienteId vinculado.
/// </summary>
public class CreateSaleDto
{
    public int? ClienteId { get; set; }

    [Required(ErrorMessage = "Debe incluir al menos un producto en la venta")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un producto en la venta")]
    public List<CreateSaleItemDto> Items { get; set; } = new();
}
