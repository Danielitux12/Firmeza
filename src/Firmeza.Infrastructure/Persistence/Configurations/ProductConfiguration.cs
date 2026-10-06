using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    // Configura la tabla y restricciones de la entidad Product.
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.Stock).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.Category).HasMaxLength(100).IsRequired();
        builder.Property(p => p.IsAvailable).HasDefaultValue(true);

        // Relación 1-N: Un producto puede estar en múltiples detalles de venta.
        builder.HasMany(p => p.SaleDetails)
            .WithOne(sd => sd.Product)
            .HasForeignKey(sd => sd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
