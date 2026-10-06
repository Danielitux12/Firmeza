namespace Firmeza.Application.ViewModels.Customers;

/// <summary>
/// Modelo de vista para el listado y búsqueda de clientes por nombre o documento.
/// </summary>
public class CustomerFilterViewModel
{
    // Término de búsqueda por nombre o número de documento.
    public string? SearchTerm { get; set; }

    // Listado de clientes encontrados.
    public List<CustomerViewModel> Customers { get; set; } = new();
}
