using Firmeza.Domain.Entities;

namespace Firmeza.Tests;

public class CustomerTests
{
    // Verifica que un cliente con datos válidos pase la validación IsValid.
    [Fact]
    public void Customer_WithValidData_ReturnsTrue()
    {
        // Organizar (Arrange)
        var customer = new Customer
        {
            Id = 1,
            FullName = "Juan Perez",
            DocumentNumber = "12345678",
            Email = "juan.perez@example.com",
            Phone = "987654321",
            Address = "Av. Principal 123"
        };

        // Actuar (Act) & Afirmar (Assert)
        Assert.True(customer.IsValid());
    }

    // Verifica que un cliente sin documento o correo no sea válido.
    [Fact]
    public void Customer_MissingRequiredData_ReturnsFalse()
    {
        // Organizar (Arrange)
        var customer = new Customer
        {
            Id = 2,
            FullName = "Empresa SAC",
            DocumentNumber = "",
            Email = ""
        };

        // Actuar (Act) & Afirmar (Assert)
        Assert.False(customer.IsValid());
    }

    // Verifica que un texto no numérico en edad arroje FormatException para ser capturado por try-catch.
    [Theory]
    [InlineData("veinticinco")]
    [InlineData("30.5")]
    [InlineData("abc")]
    public void AgeText_InvalidFormat_ThrowsFormatException(string invalidAge)
    {
        // Actuar & Afirmar
        Assert.Throws<FormatException>(() => int.Parse(invalidAge));
    }

    // Verifica que un número válido en texto sea parseado correctamente a entero.
    [Fact]
    public void AgeText_ValidNumber_ParsesCorrectly()
    {
        // Actuar
        int age = int.Parse("28");

        // Afirmar
        Assert.Equal(28, age);
    }
}
