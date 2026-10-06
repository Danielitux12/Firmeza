using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    // Configura la tabla y restricciones de la entidad Customer.
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.DocumentNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Address).HasMaxLength(250);
        builder.Property(c => c.UserId).HasMaxLength(450);

        // DocumentNumber y Email deben ser únicos según requerimiento.
        builder.HasIndex(c => c.DocumentNumber).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();

        // Relación 1-N: Un cliente puede tener múltiples ventas.
        builder.HasMany(c => c.Sales)
            .WithOne(s => s.Customer)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
