namespace Firmeza.Application.DTOs.Sales;

/// <summary>
/// Modelo de datos para consultar o responder información de una venta.
/// </summary>
public class SaleDto
{
    public int Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string? ReceiptPath { get; set; }
    public List<SaleDetailDto> Details { get; set; } = new();
}
