namespace Firmeza.Domain.Entities;

public class SaleDetail : EntityBase
{
    public int SaleId { get; set; }

    public Sale? Sale { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public void CalculateLineTotal()
    {
        LineTotal = Quantity * UnitPrice;
    }
}
