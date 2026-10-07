using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class TrabajadorConfiguration : IEntityTypeConfiguration<Trabajador>
{
    public void Configure(EntityTypeBuilder<Trabajador> builder)
    {
        builder.ToTable("trabajadores");

        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
        builder.Property(t => t.DocumentNumber).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Position).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Salary).HasPrecision(18, 2).IsRequired();
        builder.Property(t => t.Email).IsRequired().HasMaxLength(150);
        builder.Property(t => t.Phone).HasMaxLength(30);
        builder.Property(t => t.Address).HasMaxLength(250);
        builder.Property(t => t.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(t => t.DocumentNumber).IsUnique();
        builder.HasIndex(t => t.Email).IsUnique();
    }
}