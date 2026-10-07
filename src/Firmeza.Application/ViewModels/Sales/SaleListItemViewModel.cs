namespace Firmeza.Application.ViewModels.Sales;

/// <summary>
/// Modelo de vista para mostrar cada registro de venta en el listado del panel.
/// </summary>
public class SaleListItemViewModel
{
    // Identificador único de la venta.
    public int Id { get; set; }

    // Número correlativo de comprobante.
    public string SaleNumber { get; set; } = string.Empty;

    // Fecha en la que se efectuó la venta.
    public DateTime Date { get; set; }

    // Nombre completo del cliente.
    public string ClienteName { get; set; } = string.Empty;

    // Documento del cliente.
    public string ClienteDocument { get; set; } = string.Empty;

    // Cantidad total de líneas o productos comprados.
    public int TotalItems { get; set; }

    // Subtotal sin impuesto.
    public decimal Subtotal { get; set; }

    // IVA (19%).
    public decimal Tax { get; set; }

    // Importe total.
    public decimal Total { get; set; }

    // Ruta del recibo en caso de existir.
    public string? ReceiptPath { get; set; }
}
