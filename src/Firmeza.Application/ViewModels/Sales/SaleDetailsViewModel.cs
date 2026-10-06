namespace Firmeza.Application.ViewModels.Sales;

/// <summary>
/// Modelo de vista para mostrar la información completa de una venta específica.
/// </summary>
public class SaleDetailsViewModel
{
    public int Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    // Datos del cliente
    public string ClienteName { get; set; } = string.Empty;
    public string ClienteDocument { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClientePhone { get; set; } = string.Empty;

    // Totales calculados
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string? ReceiptPath { get; set; }

    // Líneas de la venta
    public List<SaleDetailItemViewModel> Items { get; set; } = new();
}
