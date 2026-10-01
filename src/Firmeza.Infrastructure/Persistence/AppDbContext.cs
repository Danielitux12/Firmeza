using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    // Contexto principal de la base de datos.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Colección de productos.
    public DbSet<Product> Products => Set<Product>();

    // Colección de empleados.
    public DbSet<Employee> Employees => Set<Employee>();

    // Configura modelos y relaciones de la base de datos.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}