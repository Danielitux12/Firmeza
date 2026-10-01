using Firmeza.Configuration;
using Firmeza.Infrastructure;
using Firmeza.Infrastructure.Persistence;
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

var app = builder.Build();

// Aplica migraciones pendientes automáticamente en la base de datos PostgreSQL
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configura el pipeline HTTP de la aplicación.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // El valor predeterminado de HSTS es 30 días. Puedes cambiarlo según tu entorno de producción.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();