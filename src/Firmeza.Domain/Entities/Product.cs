namespace Firmeza.Domain.Entities;

public class Product
{
    // Identificador del producto.
    public int Id { get; set; }

    // Nombre del producto.
    public string Name { get; set; } = string.Empty;

    // Precio del producto.
    public decimal Price { get; set; }

    // Indica si el producto está disponible para venta.
    public bool IsAvailable { get; set; } = true;

    // Marca el producto como no disponible.
    public void MarkAsUnavailable()
    {
        IsAvailable = false;
    }

    // Marca el producto como disponible.
    public void MarkAsAvailable()
    {
        IsAvailable = true;
    }

    // Valida que el producto tenga información correcta.
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && Price >= 0;
    }
}