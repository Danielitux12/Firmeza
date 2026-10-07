namespace Firmeza.Application.ViewModels.Dashboard;

/// <summary>
/// Modelo de vista para las tarjetas principales del panel de administración.
/// </summary>
public class DashboardViewModel
{
    // Cantidad total de productos registrados en catálogo.
    public int TotalProducts { get; set; }

    // Cantidad total de clientes registrados.
    public int TotalClientes { get; set; }

    // Cantidad total de ventas realizadas.
    public int TotalSales { get; set; }

    // Monto acumulado total de ventas.
    public decimal TotalSalesRevenue { get; set; }
}
