namespace Firmeza.Domain.Entities;

public class Sale : EntityBase
{
    public string SaleNumber { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public string? ReceiptPath { get; set; }

    public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    public void CalculateTotals()
    {
        Subtotal = 0;
        foreach (var detail in SaleDetails)
        {
            detail.CalculateLineTotal();
            Subtotal += detail.LineTotal;
        }

        Tax = Math.Round(Subtotal * 0.19m, 2);
        Total = Subtotal + Tax;
    }
}
