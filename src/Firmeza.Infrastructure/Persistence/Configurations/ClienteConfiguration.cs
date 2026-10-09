using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    // Configura la tabla y restricciones de la entidad Cliente.
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.DocumentNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Address).HasMaxLength(250);
        builder.Property(c => c.UserId).HasMaxLength(450);
        builder.Property(c => c.IsActive).HasDefaultValue(true).IsRequired();

        // DocumentNumber y Email deben ser únicos según requerimiento.
        builder.HasIndex(c => c.DocumentNumber).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();
    }
}