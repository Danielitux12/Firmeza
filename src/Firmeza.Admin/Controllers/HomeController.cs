using System.Diagnostics;
using Firmeza.Admin.Models;
using Firmeza.Application.ViewModels.Dashboard;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Admin.Controllers;

/// <summary>
/// Controlador principal del panel administrativo. Muestra el resumen del Dashboard.
/// </summary>
[Authorize(Roles = "Administrador")]
public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    // Muestra el panel principal con los totales de productos, clientes y ventas.
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            // Cuenta el total de productos en catálogo.
            TotalProducts = await _context.Products.CountAsync(),

            // Cuenta el total de clientes registrados.
            TotalCustomers = await _context.Customers.CountAsync(),

            // Cuenta el total de ventas generadas.
            TotalSales = await _context.Sales.CountAsync(),

            // Suma el total recaudado en ventas (0 si no hay registros).
            TotalSalesRevenue = await _context.Sales.SumAsync(s => (decimal?)s.Total) ?? 0m
        };

        return View(model);
    }

    // Muestra la página de privacidad.
    public IActionResult Privacy()
    {
        return View();
    }

    // Muestra la vista de error ante excepciones no controladas.
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}