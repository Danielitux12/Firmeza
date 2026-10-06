namespace Firmeza.Application.ViewModels.Sales;

/// <summary>
/// Modelo de vista para mostrar una fila de producto en el detalle de una venta.
/// </summary>
public class SaleDetailItemViewModel
{
    // Identificador del producto.
    public int ProductId { get; set; }

    // Nombre comercial del producto.
    public string ProductName { get; set; } = string.Empty;

    // Cantidad de unidades vendidas.
    public int Quantity { get; set; }

    // Precio unitario aplicado.
    public decimal UnitPrice { get; set; }

    // Total de la línea (Quantity * UnitPrice).
    public decimal LineTotal { get; set; }
}
