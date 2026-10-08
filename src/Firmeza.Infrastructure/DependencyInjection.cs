using Firmeza.Application.Interfaces;
using Firmeza.Application.Services;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Persistence.Repositories;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        // 1. Contexto con PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // 2. ASP.NET Core Identity
        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

        // 3. Repositorio Genérico para entidades de dominio (Cliente, Empresa)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // 4. Servicios de Aplicación (CRUDs compartidos)
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IEmpresaService, EmpresaService>();

        // 5. Utilidades y exportaciones
        services.AddScoped<IExcelExporter, ExcelExporter>();
        services.AddScoped<IPdfExporter, PdfExporter>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}