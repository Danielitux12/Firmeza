using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Infrastructure.Persistence;

/// <summary>
/// Se encarga de inicializar datos indispensables en la base de datos (roles y administrador).
/// </summary>
public static class DatabaseSeeder
{
    // Ejecuta la siembra de roles y del usuario administrador inicial.
    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // 1. Crear los roles obligatorios: "Administrador" y "Cliente".
        string[] roles = ["Administrador", "Cliente"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Obtener credenciales del administrador desde variables de entorno o configuración.
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
                         ?? configuration["Admin:Email"]
                         ?? "admin@firmeza.com";

        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                            ?? configuration["Admin:Password"]
                            ?? "Admin123*";

        // 3. Crear el usuario administrador si no existe todavía.
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                // Asigna el rol Administrador al usuario creado.
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }
    }
}
