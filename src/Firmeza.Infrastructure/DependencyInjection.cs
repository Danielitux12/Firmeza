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
    // Registra la infraestructura de acceso a datos y servicios concretos.
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        // Configura el contexto de base de datos con PostgreSQL.
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Configura Identity con soporte para roles y EF Core.
        services.AddIdentityCore<IdentityUser>(options =>
        {
            // Opciones de contraseña simples para ambiente académico.
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

        // Registra repositorios e implementaciones de aplicación.
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ProductService>();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IEmpresaService, EmpresaService>();

        // Registra servicios de Excel (EPPlus) y PDF (QuestPDF).
        services.AddScoped<IExcelImporter, ExcelImporter>();
        services.AddScoped<IExcelExporter, ExcelExporter>();
        services.AddScoped<IPdfExporter, PdfExporter>();
        services.AddScoped<IReceiptGenerator, ReceiptGenerator>();

        // Registra el servicio de correo SMTP
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}