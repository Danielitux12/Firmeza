namespace Firmeza.Application.ViewModels.Products;

/// <summary>
/// Modelo de vista para los filtros de búsqueda en el listado de productos.
/// </summary>
public class ProductFilterViewModel
{
    // Texto de búsqueda por nombre del producto.
    public string? SearchName { get; set; }

    // Categoría seleccionada para filtrar.
    public string? SelectedCategory { get; set; }

    // Estado de disponibilidad seleccionado (null = todos, true = disponibles, false = no disponibles).
    public bool? AvailabilityFilter { get; set; }

    // Lista de categorías únicas existentes para cargar en el dropdown de filtros.
    public List<string> AvailableCategories { get; set; } = new();

    // Lista de productos resultantes.
    public List<ProductViewModel> Products { get; set; } = new();
}
