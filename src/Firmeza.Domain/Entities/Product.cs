namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa un producto disponible en el inventario de la ferretería.
/// </summary>
public class Product
{
    // Identificador único del producto.
    public int Id { get; set; }

    // Nombre comercial del producto.
    public string Name { get; set; } = string.Empty;

    // Descripción detallada del producto.
    public string Description { get; set; } = string.Empty;

    // Precio unitario de venta.
    public decimal Price { get; set; }

    // Cantidad disponible en stock.
    public int Stock { get; set; }

    // Categoría a la que pertenece el producto (ej. Cemento, Herramientas, etc.).
    public string Category { get; set; } = string.Empty;

    // Indica si el producto está disponible para la venta.
    public bool IsAvailable { get; set; } = true;

    // Colección de detalles de venta asociados a este producto.
    public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    // Valida que el producto tenga datos correctos y precio no negativo.
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && Price >= 0
            && Stock >= 0;
    }

    // Verifica si hay existencias suficientes para satisfacer la cantidad solicitada.
    public bool HasStock(int quantity)
    {
        return IsAvailable && Stock >= quantity && quantity > 0;
    }

    // Reduce el stock según la cantidad vendida.
    public void ReduceStock(int quantity)
    {
        if (!HasStock(quantity))
        {
            throw new InvalidOperationException($"Stock insuficiente para el producto '{Name}'.");
        }

        Stock -= quantity;
        if (Stock == 0)
        {
            IsAvailable = false;
        }
    }
}