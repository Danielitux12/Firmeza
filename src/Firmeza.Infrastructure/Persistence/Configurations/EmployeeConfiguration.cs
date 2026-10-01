using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firmeza.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.Position).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Salary).HasPrecision(18, 2);
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasIndex(e => e.DocumentNumber).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
    }
}