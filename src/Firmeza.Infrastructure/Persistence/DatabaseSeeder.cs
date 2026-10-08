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

        // 4. Crear el usuario cliente inicial si no existe todavía.
        var clientEmail = Environment.GetEnvironmentVariable("CLIENT_EMAIL")
                         ?? configuration["Client:Email"]
                         ?? "cliente@firmeza.com";

        var clientPassword = Environment.GetEnvironmentVariable("CLIENT_PASSWORD")
                            ?? configuration["Client:Password"]
                            ?? "Cliente123*";

        var clientUser = await userManager.FindByEmailAsync(clientEmail);
        if (clientUser is null)
        {
            clientUser = new IdentityUser
            {
                UserName = clientEmail,
                Email = clientEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(clientUser, clientPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(clientUser, "Cliente");
            }
        }

        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        if (!await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(dbContext.Clientes, c => c.Email == clientEmail))
        {
            var cliente = new Domain.Entities.Cliente
            {
                Name = "Cliente Principal",
                DocumentNumber = "1000000001",
                Email = clientEmail,
                Phone = "3001234567",
                Address = "Calle 10 # 20 - 30",
                BirthDate = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UserId = clientUser?.Id
            };
            cliente.Activate();
            dbContext.Clientes.Add(cliente);
            await dbContext.SaveChangesAsync();
        }
    }
}
