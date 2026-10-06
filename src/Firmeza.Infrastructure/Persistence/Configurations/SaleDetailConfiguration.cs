using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    // Configura la tabla y restricciones de la entidad SaleDetail.
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("sale_details");

        builder.HasKey(sd => sd.Id);
        builder.Property(sd => sd.Quantity).IsRequired();
        builder.Property(sd => sd.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(sd => sd.LineTotal).HasPrecision(18, 2).IsRequired();

        // Relación N-1 con Sale.
        builder.HasOne(sd => sd.Sale)
            .WithMany(s => s.SaleDetails)
            .HasForeignKey(sd => sd.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación N-1 con Product.
        builder.HasOne(sd => sd.Product)
            .WithMany(p => p.SaleDetails)
            .HasForeignKey(sd => sd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
