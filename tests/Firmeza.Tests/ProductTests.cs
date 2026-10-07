using Firmeza.Domain.Entities;
using Firmeza.Domain.Enums;

namespace Firmeza.Tests;

public class ProductTests
{
    // Verifica que se pueda instanciar un producto con todas sus propiedades y relaciones.
    [Fact]
    public void Product_Creation_SetsPropertiesCorrectly()
    {
        // Organizar (Arrange) y Actuar (Act)
        var product = new Product
        {
            Id = 1,
            Name = "Cemento Portland",
            Description = "Bolsa de 50kg para construcción",
            Price = 28.50m,
            Stock = 100,
            Category = "Construcción",
            Status = ProductStatus.Available
        };

        // Afirmar (Assert)
        Assert.Equal(1, product.Id);
        Assert.Equal("Cemento Portland", product.Name);
        Assert.Equal(28.50m, product.Price);
        Assert.Equal(100, product.Stock);
        Assert.True(product.IsValid());
        Assert.True(product.HasStock(10));
    }

    // Verifica que reducir el stock descuente la cantidad y marque como no disponible si llega a cero.
    [Fact]
    public void Product_ReduceStock_UpdatesStockAndAvailability()
    {
        // Organizar (Arrange)
        var product = new Product
        {
            Id = 1,
            Name = "Pintura Blanca",
            Price = 50.00m,
            Stock = 5,
            Status = ProductStatus.Available
        };

        // Actuar (Act)
        product.ReduceStock(5);

        // Afirmar (Assert)
        Assert.Equal(0, product.Stock);
        Assert.Equal(ProductStatus.Unavailable, product.Status);
    }

    [Theory]
    [InlineData(ProductStatus.Unavailable)]
    [InlineData(ProductStatus.Discontinued)]
    public void Product_HasStock_RejectsNonSellableStatuses(ProductStatus status)
    {
        var product = new Product
        {
            Name = "Taladro",
            Stock = 5,
            Status = status
        };

        Assert.False(product.HasStock(1));
    }
}
