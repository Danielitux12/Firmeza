namespace Firmeza.Application.ViewModels.Dashboard;

/// <summary>
/// Modelo de vista para el panel de administración enfocado en Clientes y Empresas.
/// </summary>
public class DashboardViewModel
{
    public int TotalClientes { get; set; }
    public int TotalClientesActivos { get; set; }
    public int TotalEmpresas { get; set; }
    public int TotalEmpresasActivas { get; set; }
}
