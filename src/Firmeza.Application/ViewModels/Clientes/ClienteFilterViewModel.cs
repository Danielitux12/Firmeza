namespace Firmeza.Application.ViewModels.Clientes;

/// <summary>
/// Modelo de vista para el listado y búsqueda de clientes por nombre o documento.
/// </summary>
public class ClienteFilterViewModel
{
    // Término de búsqueda por nombre o número de documento.
    public string? SearchTerm { get; set; }

    public string Status { get; set; } = "active";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    // Listado de clientes encontrados.
    public List<ClienteViewModel> Clientes { get; set; } = new();
}
