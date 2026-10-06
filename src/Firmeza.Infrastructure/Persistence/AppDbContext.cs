using Firmeza.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    // Contexto principal de la base de datos con soporte para ASP.NET Core Identity.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Colección de productos.
    public DbSet<Product> Products => Set<Product>();

    // Colección de clientes.
    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Empresa> Empresas => Set<Empresa>();

    public DbSet<Trabajador> Trabajadores => Set<Trabajador>();

    // Colección de ventas.
    public DbSet<Sale> Sales => Set<Sale>();

    // Colección de detalles de venta.
    public DbSet<SaleDetail> SaleDetails => Set<SaleDetail>();

    // Configura modelos, relaciones y las tablas de Identity.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Obligatorio llamar a base.OnModelCreating para registrar las entidades de Identity.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EntityBase>().UseTpcMappingStrategy();

        // Aplica todas las configuraciones IEntityTypeConfiguration de este ensamblado.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}