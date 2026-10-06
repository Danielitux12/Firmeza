namespace Firmeza.Application.DTOs.Sales;

/// <summary>
/// Representa el detalle individual de una venta en las respuestas del API.
/// </summary>
public class SaleDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
