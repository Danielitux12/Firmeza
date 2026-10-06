using Firmeza.Admin.Configuration;
using Firmeza.Infrastructure;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Carga el archivo .env si existe; solo se usa en desarrollo local.
EnvironmentConfiguration.LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

// Agrega servicios MVC y controladores de la aplicación.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta configurar la cadena de conexión 'ConnectionStrings:DefaultConnection'. " +
        "Define la variable de entorno ConnectionStrings__DefaultConnection o agrégala en un archivo .env o appsettings.Development.json.");
}

// Registra infraestructura y acceso a datos.
builder.Services.AddInfrastructure(connectionString);

// Configura Identity con soporte para cookies y roles.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configura las rutas y opciones de la cookie de autenticación para el panel.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "Firmeza.Admin.Auth";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Aplica migraciones pendientes y ejecuta el seed inicial automáticamente si la BD está disponible
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();

        // Ejecuta la siembra de roles y del usuario administrador
        DatabaseSeeder.SeedAsync(scope.ServiceProvider, app.Configuration).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Aviso: No se pudo conectar a la base de datos PostgreSQL en 'localhost:5432'. Las migraciones y el seed no se ejecutaron.");
    }
}

// Configura el pipeline HTTP de la aplicación.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilita autenticación y autorización en el orden requerido.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();