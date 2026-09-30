using Firmeza.Application.Interfaces;
using Firmeza.Application.Services;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Infrastructure;

public static class DependencyInjection
{
    // Registra la infraestructura de acceso a datos y servicios concretos.
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        // Configura el contexto de base de datos con PostgreSQL.
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Registra repositorios e implementaciones de aplicación.
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<EmployeeService>();
        services.AddScoped<ProductService>();

        return services;
    }
}