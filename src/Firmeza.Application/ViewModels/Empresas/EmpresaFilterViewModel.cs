namespace Firmeza.Application.ViewModels.Empresas;

public class EmpresaFilterViewModel
{
    public string? Search { get; set; }
    public string Status { get; set; } = "active";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<EmpresaViewModel> Items { get; set; } = new();
}
