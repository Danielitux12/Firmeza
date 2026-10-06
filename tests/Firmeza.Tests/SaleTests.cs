using Firmeza.Domain.Entities;

namespace Firmeza.Tests;

public class SaleTests
{
    // Verifica que el método CalculateTotals compute subtotal, 19% de IVA y total correctamente.
    [Fact]
    public void Sale_CalculateTotals_ComputesSubtotalTaxAndTotalCorrectly()
    {
        // Organizar (Arrange)
        var sale = new Sale
        {
            Id = 1,
            SaleNumber = "VTA-00001",
            CustomerId = 10,
            SaleDetails = new List<SaleDetail>
            {
                new SaleDetail
                {
                    Id = 1,
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 50.00m
                },
                new SaleDetail
                {
                    Id = 2,
                    ProductId = 2,
                    Quantity = 1,
                    UnitPrice = 100.00m
                }
            }
        };

        // Actuar (Act)
        sale.CalculateTotals();

        // Afirmar (Assert)
        // Subtotal = (2 * 50) + (1 * 100) = 200.00
        // IVA 19%  = 200 * 0.19 = 38.00
        // Total    = 200 + 38 = 238.00
        Assert.Equal(200.00m, sale.Subtotal);
        Assert.Equal(38.00m, sale.Tax);
        Assert.Equal(238.00m, sale.Total);
    }
}
