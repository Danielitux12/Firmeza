namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa una venta o transacción realizada en la ferretería.
/// </summary>
public class Sale
{
    // Identificador único de la venta.
    public int Id { get; set; }

    // Código o número correlativo único de la venta (ej. VTA-00001).
    public string SaleNumber { get; set; } = string.Empty;

    // Fecha y hora en la que se efectuó la venta.
    public DateTime Date { get; set; } = DateTime.UtcNow;

    // Identificador del cliente que realizó la compra.
    public int CustomerId { get; set; }

    // Referencia de navegación al cliente.
    public Customer? Customer { get; set; }

    // Subtotal antes de impuestos.
    public decimal Subtotal { get; set; }

    // Impuesto al valor agregado (IVA 19%).
    public decimal Tax { get; set; }

    // Importe total a pagar (Subtotal + Tax).
    public decimal Total { get; set; }

    // Ruta relativa en disco o almacenamiento al comprobante PDF generado.
    public string? ReceiptPath { get; set; }

    // Líneas o detalles que componen la venta.
    public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    // Calcula el subtotal, el IVA (19%) y el total de la venta sumando las líneas.
    public void CalculateTotals()
    {
        Subtotal = 0;
        foreach (var detail in SaleDetails)
        {
            detail.CalculateLineTotal();
            Subtotal += detail.LineTotal;
        }

        // IVA correspondiente al 19% en Colombia/región según requerimiento.
        Tax = Math.Round(Subtotal * 0.19m, 2);
        Total = Subtotal + Tax;
    }
}
