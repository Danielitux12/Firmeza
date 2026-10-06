using Firmeza.Domain.Entities;
using Firmeza.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    // Configura la tabla y restricciones de la entidad Product.
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.Stock).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.Category).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(p => p.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasOne(p => p.Empresa)
            .WithMany(e => e.Products)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relación 1-N: Un producto puede estar en múltiples detalles de venta.
        builder.HasMany(p => p.SaleDetails)
            .WithOne(sd => sd.Product)
            .HasForeignKey(sd => sd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
