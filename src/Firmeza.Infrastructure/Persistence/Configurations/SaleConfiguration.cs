using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    // Configura la tabla y restricciones de la entidad Sale.
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.SaleNumber).IsUnique();

        builder.Property(s => s.Date).IsRequired();
        builder.Property(s => s.Subtotal).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.Tax).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.Total).HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.ReceiptPath).HasMaxLength(500);
        builder.Property(s => s.IsActive).HasDefaultValue(true).IsRequired();

        // Relación N-1: Cada venta pertenece a un cliente.
        builder.HasOne(s => s.Cliente)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación 1-N: Una venta contiene varios detalles (eliminación en cascada para detalles).
        builder.HasMany(s => s.SaleDetails)
            .WithOne(sd => sd.Sale)
            .HasForeignKey(sd => sd.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
