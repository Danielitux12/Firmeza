using Firmeza.Application.Interfaces;
using Firmeza.Application.Services;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        {
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        // Registra repositorios e implementaciones de aplicación.
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<EmployeeService>();
        services.AddScoped<ProductService>();

        return services;
    }
}