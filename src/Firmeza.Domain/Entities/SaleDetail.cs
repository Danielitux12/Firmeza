namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa el detalle individual (línea de producto) de una venta.
/// </summary>
public class SaleDetail
{
    // Identificador único del detalle.
    public int Id { get; set; }

    // Identificador de la venta a la que pertenece.
    public int SaleId { get; set; }

    // Referencia de navegación a la venta padre.
    public Sale? Sale { get; set; }

    // Identificador del producto vendido.
    public int ProductId { get; set; }

    // Referencia de navegación al producto vendido.
    public Product? Product { get; set; }

    // Cantidad de unidades vendidas.
    public int Quantity { get; set; }

    // Precio unitario aplicado al momento de la venta.
    public decimal UnitPrice { get; set; }

    // Importe total de la línea (Quantity * UnitPrice).
    public decimal LineTotal { get; set; }

    // Calcula el total de la línea a partir de la cantidad y el precio unitario.
    public void CalculateLineTotal()
    {
        LineTotal = Quantity * UnitPrice;
    }
}
