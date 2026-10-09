using Firmeza.Application.ViewModels.Dashboard;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers;

/// <summary>
/// Proporciona métricas consolidadas del sistema para el panel de administración.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SoloAdministrador")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retorna los conteos totales y activos de Clientes y Empresas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DashboardViewModel>> GetMetrics(CancellationToken cancellationToken = default)
    {
        var model = new DashboardViewModel
        {
            TotalClientes = await _context.Clientes.AsNoTracking().CountAsync(cancellationToken),
            TotalClientesActivos = await _context.Clientes.AsNoTracking().CountAsync(c => c.IsActive, cancellationToken),
            TotalEmpresas = await _context.Empresas.AsNoTracking().CountAsync(cancellationToken),
            TotalEmpresasActivas = await _context.Empresas.AsNoTracking().CountAsync(e => e.IsActive, cancellationToken)
        };

        return Ok(model);
    }
}
