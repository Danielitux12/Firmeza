using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Infrastructure.Persistence;

/// <summary>
/// Se encarga de inicializar datos indispensables en la base de datos (roles, administradores, clientes y empresas de prueba).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration, CancellationToken cancellationToken = default)
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

        // 5. Sembrar 18 Clientes con datos variados (densidad, paginación y estados mixtos)
        var sampleClientes = new (string Name, string Doc, string Email, string Phone, string Address, DateTime BirthDate, bool IsActive)[]
        {
            ("Cliente Principal", "1000000001", clientEmail, "3001234567", "Calle 10 # 20 - 30", new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc), true),
            ("Alejandro Morales Restrepo", "1020304050", "amorales@empresa.com", "+57 310 456 7890", "Avenida Carrera 19 # 104 - 58 Oficina 502, Torre Santa Bárbara", new DateTime(1988, 4, 12, 0, 0, 0, DateTimeKind.Utc), true),
            ("Beatriz Elena Salazar de los Ríos", "1032456789", "bsalazar@consulting.co", "+57 315 889 0012", "Calle 72 # 11 - 45 Edificio Grancolombiana Piso 8", new DateTime(1991, 8, 23, 0, 0, 0, DateTimeKind.Utc), true),
            ("Carlos Eduardo Villamizar Peña", "1018765432", "cvillamizar@gmail.com", "+57 320 112 3344", "Carrera 15 # 85 - 20 Apartamento 401, Chicó Norte", new DateTime(1982, 11, 5, 0, 0, 0, DateTimeKind.Utc), true),
            ("Diana Marcela Rodríguez Gutiérrez", "1098234567", "diana.rodriguez@outlook.com", "+57 301 998 7766", "Diagonal 45A # 16A - 22 Barrio Palermo", new DateTime(1996, 2, 17, 0, 0, 0, DateTimeKind.Utc), true),
            ("Ernesto Gómez Jaramillo", "1045678901", "ernesto.gomez@corporativo.com", "+57 312 334 5566", "Transversal 23 # 97 - 74 Interior 3, El Virrey", new DateTime(1975, 9, 30, 0, 0, 0, DateTimeKind.Utc), false), // Suspendido
            ("Fernanda Lucía Cárdenas Ospina", "1056789012", "fernanda.cardenas@servicios.net", "+57 318 765 4321", "Avenida Las Palmas Kilómetro 7 Condominio Bosques del Este", new DateTime(1999, 6, 14, 0, 0, 0, DateTimeKind.Utc), true),
            ("Guillermo Alfonso Paternina Cuello", "1067890123", "gpaternina@inversiones.com", "+57 300 876 5432", "Calle 100 # 8A - 55 Torre B Piso 14", new DateTime(1980, 12, 1, 0, 0, 0, DateTimeKind.Utc), true),
            ("Helena Patricia Zuluaga Castaño", "1078901234", "helena.zuluaga@zuluagaabogados.com", "+57 314 223 4455", "Carrera 7 # 156 - 68 Torre C Oficina 801, Cedritos", new DateTime(1985, 3, 28, 0, 0, 0, DateTimeKind.Utc), true),
            ("Ignacio Andrés Betancur Londoño", "1089012345", "ignacio.betancur@industrias.co", "+57 316 667 8899", "Calle 26 # 69D - 91 Complejo Logístico Salitre", new DateTime(1990, 7, 9, 0, 0, 0, DateTimeKind.Utc), false), // Suspendido
            ("Juliana María Echeverri Monsalve", "1090123456", "juliana.echeverri@medios.com", "+57 305 443 2211", "Carrera 43A # 1Sur - 220 Edificio San Fernando Plaza", new DateTime(1993, 10, 19, 0, 0, 0, DateTimeKind.Utc), true),
            ("Katherin Johanna Vargas Moncada", "1091234567", "kvargas@logistica.com.co", "+57 311 556 7788", "Avenida El Dorado # 103 - 09 Bodega 4 Parque Industrial", new DateTime(1997, 5, 4, 0, 0, 0, DateTimeKind.Utc), true),
            ("Luis Felipe Santander Quintero", "1092345678", "luis.santander@santander.org", "+57 317 889 9001", "Calle 93B # 13 - 42 Consultorio 305, Parque de la 93", new DateTime(1978, 8, 15, 0, 0, 0, DateTimeKind.Utc), true),
            ("Mónica Viviana Carvajal Arboleda", "1093456789", "monica.carvajal@carvajal.com", "+57 313 112 2334", "Carrera 9 # 113 - 52 Torre Scotia Plaza", new DateTime(1987, 1, 25, 0, 0, 0, DateTimeKind.Utc), false), // Suspendido
            ("Néstor Javier Caicedo Barrientos", "1094567890", "ncaicedo@infraestructura.co", "+57 319 990 0112", "Calle 140 # 11 - 38 Apartamento 502, Belmira", new DateTime(1983, 11, 11, 0, 0, 0, DateTimeKind.Utc), true),
            ("Olga Lucía Samper Bermúdez", "1095678901", "olga.samper@samper.com", "+57 304 332 1100", "Avenida Boyacá # 138 - 45 Manzana 3 Casa 12", new DateTime(1994, 9, 3, 0, 0, 0, DateTimeKind.Utc), true),
            ("Pablo Emilio Rincón Valderrama", "1096789012", "pablo.rincon@comercial.com", "+57 310 221 1009", "Carrera 68D # 13 - 54 Local 102, Fontibón", new DateTime(1986, 4, 18, 0, 0, 0, DateTimeKind.Utc), false), // Suspendido
            ("Raquel Sofía Domínguez Tamayo", "1097890123", "raquel.dominguez@tamayo.com", "+57 318 445 5667", "Calle 116 # 7 - 15 Oficina 604, Santa Ana Occidental", new DateTime(1992, 12, 30, 0, 0, 0, DateTimeKind.Utc), true),
            ("Santiago Andrés Restrepo Cardona", "1098901234", "santiago.restrepo@cardonagroup.com", "+57 300 554 1122", "Circular 4 # 73 - 88 Laureles", new DateTime(1990, 3, 15, 0, 0, 0, DateTimeKind.Utc), true),
            ("Tatiana Rocío Valencia Beltrán", "1099012345", "tvalencia@ingenieria.co", "+57 314 887 9900", "Carrera 51B # 82 - 254 Prado", new DateTime(1989, 7, 22, 0, 0, 0, DateTimeKind.Utc), true),
            ("Uriel Fernando Castañeda Marín", "1088123456", "uriel.castaneda@marin.net", "+57 316 223 3344", "Calle 50 # 13 - 24 Centro", new DateTime(1979, 10, 10, 0, 0, 0, DateTimeKind.Utc), false), // Suspendido
            ("Valeria Sofía Navas Quintero", "1077234567", "valeria.navas@soluciones.com", "+57 311 445 6677", "Avenida Santander # 55 - 12 Torre Médica", new DateTime(1998, 9, 5, 0, 0, 0, DateTimeKind.Utc), true),
            ("William Alexander Prado Ortiz", "1066345678", "wprado@pradoconsultores.com", "+57 317 778 8899", "Calle 18 # 102 - 15 Ciudad Jardín", new DateTime(1984, 6, 19, 0, 0, 0, DateTimeKind.Utc), true),
            ("Ximena Andrea Londoño Zapata", "1055456789", "ximena.londono@zapata.co", "+57 302 334 4455", "Carrera 27 # 36 - 14 Cabecera del Llano", new DateTime(1995, 11, 28, 0, 0, 0, DateTimeKind.Utc), true)
        };

        foreach (var item in sampleClientes)
        {
            if (!await dbContext.Clientes.AnyAsync(c => c.Email == item.Email || c.DocumentNumber == item.Doc, cancellationToken))
            {
                var cliente = new Domain.Entities.Cliente
                {
                    Name = item.Name,
                    DocumentNumber = item.Doc,
                    Email = item.Email,
                    Phone = item.Phone,
                    Address = item.Address,
                    BirthDate = item.BirthDate,
                    UserId = item.Email == clientEmail ? clientUser?.Id : null
                };

                if (item.IsActive)
                {
                    cliente.Activate();
                }
                else
                {
                    cliente.Deactivate();
                }

                dbContext.Clientes.Add(cliente);
            }
        }

        // 6. Sembrar 12 Empresas con datos variados
        var sampleEmpresas = new (string Name, string Nit, string Email, string Phone, string Address, bool IsActive)[]
        {
            ("Inversiones y Construcciones del Caribe S.A.S.", "901234567-8", "contacto@inversionescaribe.co", "+57 601 744 5500", "Avenida Carrera 45 # 108 - 27 Torre 2 Piso 11", true),
            ("Logística Global y Transporte Terrestre Limitada", "800192837-1", "operaciones@logisticaglobal.com", "+57 601 890 1200", "Autopista Medellín Km 3.5 Parque Empresarial San Roque Bodega 12", true),
            ("Consultores Jurídicos y Financieros Andinos S.A.", "900543210-9", "atencion@andinosconsultores.com", "+57 601 319 8800", "Carrera 7 # 71 - 21 Torre B Oficina 1501, Edificio Bancafe", true),
            ("Distribuidora Nacional de Alimentos y Bebidas S.A.S.", "901889900-4", "ventas@distribuidoranal.com", "+57 602 667 9900", "Calle 13 # 68D - 35 Zona Industrial Montevideo", true),
            ("Tecnología e Innovación Digital Avanzada Ltda.", "830099112-3", "corporativo@innovaciondigital.co", "+57 604 448 3322", "Carrera 43A # 16Sur - 47 Edificio Inserte Piso 5, El Poblado", false), // Inactiva
            ("Servicios Integrales de Seguridad y Custodia S.A.", "860012345-6", "seguridad@serviciosseguridad.com", "+57 601 222 4455", "Calle 100 # 19A - 40 Edificio Capital Tower Piso 4", true),
            ("Agroindustrias del Valle del Cauca S.A.S.", "900778899-2", "gerencia@agrodelvalle.com.co", "+57 602 889 1122", "Avenida Roosevelt # 34 - 56, Cali", true),
            ("Importaciones y Suministros Hospitalarios de Colombia", "890987654-5", "importaciones@suministroshosp.com", "+57 605 385 7700", "Vía 40 # 73 - 290 Complejo Empresarial Las Flores", false), // Inactiva
            ("Soluciones Energéticas y Renovables de Colombia S.A.", "901334455-6", "info@energiasrenovables.co", "+57 601 512 8800", "Calle 72 # 7 - 84 Torre Financiera Piso 10", true),
            ("Editorial y Comunicaciones del Pacífico S.A.S.", "800332211-7", "contacto@editorialpacifico.com", "+57 602 485 6600", "Carrera 4 # 10 - 44 Centro Histórico, Cali", true),
            ("Laboratorios Farmacéuticos de Occidente Ltda.", "860554433-2", "atencionalcliente@laboccidente.com", "+57 604 312 9900", "Calle 30A # 65 - 18 Belén Industrial, Medellín", true),
            ("Red Logística de Carga Pesada Nacional S.A.S.", "901998877-1", "operaciones@redlogistica.com.co", "+57 605 311 4455", "Zona Franca de Barranquilla Módulo 8", false) // Inactiva
        };

        foreach (var item in sampleEmpresas)
        {
            if (!await dbContext.Empresas.AnyAsync(e => e.Nit == item.Nit || e.Email == item.Email, cancellationToken))
            {
                var empresa = new Domain.Entities.Empresa
                {
                    Name = item.Name,
                    Nit = item.Nit,
                    Email = item.Email,
                    Phone = item.Phone,
                    Address = item.Address
                };

                if (item.IsActive)
                {
                    empresa.Activate();
                }
                else
                {
                    empresa.Deactivate();
                }

                dbContext.Empresas.Add(empresa);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
