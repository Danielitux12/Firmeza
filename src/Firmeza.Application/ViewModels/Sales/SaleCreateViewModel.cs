using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.ViewModels.Sales;

/// <summary>
/// Modelo de vista para capturar el formulario de creación de una nueva venta.
/// </summary>
public class SaleCreateViewModel
{
    // Cliente seleccionado para la venta.
    [Required(ErrorMessage = "Debes seleccionar un cliente.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un cliente válido.")]
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }

    // Líneas de productos a vender.
    public List<SaleItemInputViewModel> Items { get; set; } = new();

    // Catálogo de clientes para el selector de la vista.
    public List<ClienteOptionViewModel> AvailableClientes { get; set; } = new();

    // Catálogo de productos disponibles con existencias para el selector.
    public List<ProductOptionViewModel> AvailableProducts { get; set; } = new();
}

/// <summary>
/// Representa una línea de producto seleccionada en el formulario de venta.
/// </summary>
public class SaleItemInputViewModel
{
    [Required(ErrorMessage = "Selecciona un producto.")]
    [Range(1, int.MaxValue, ErrorMessage = "Producto no válido.")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 10000, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Elemento para poblar el dropdown de clientes.
/// </summary>
public class ClienteOptionViewModel
{
    public int Id { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}

/// <summary>
/// Elemento para poblar el catálogo de selección de productos.
/// </summary>
public class ProductOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
