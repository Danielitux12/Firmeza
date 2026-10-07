using System.Diagnostics;
using Firmeza.Admin.Models;
using Firmeza.Application.ViewModels.Dashboard;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Admin.Controllers;

/// <summary>
/// Controlador principal del panel administrativo enfocado en Clientes y Empresas.
/// </summary>
[Authorize(Roles = "Administrador")]
public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalClientes = await _context.Clientes.CountAsync(),
            TotalClientesActivos = await _context.Clientes.CountAsync(c => c.IsActive),
            TotalEmpresas = await _context.Empresas.CountAsync(),
            TotalEmpresasActivas = await _context.Empresas.CountAsync(e => e.IsActive)
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}