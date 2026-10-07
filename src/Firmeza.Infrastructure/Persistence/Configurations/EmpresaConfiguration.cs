using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Nit).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.Address).HasMaxLength(250);
        builder.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();
        builder.HasIndex(e => e.Nit).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
    }
}