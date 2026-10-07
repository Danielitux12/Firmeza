using Firmeza.Domain.Enums;

namespace Firmeza.Domain.Entities;

public class Product : NamedEntity
{
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Category { get; set; } = string.Empty;

    public ProductStatus Status { get; set; } = ProductStatus.Available;

    public int? EmpresaId { get; set; }

    public Empresa? Empresa { get; set; }

    public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && Price >= 0
            && Stock >= 0;
    }

    public bool HasStock(int quantity)
    {
        return Status == ProductStatus.Available && Stock >= quantity && quantity > 0;
    }

    public void ReduceStock(int quantity)
    {
        if (!HasStock(quantity))
        {
            throw new InvalidOperationException($"Stock insuficiente para el producto '{Name}'.");
        }

        Stock -= quantity;
        if (Stock == 0)
        {
            Status = ProductStatus.Unavailable;
        }
    }
}