using Firmeza.Application.DTOs.Clientes;
using Firmeza.Application.Validators;
using Xunit;

namespace Firmeza.Tests;

public class ClienteTests
{
    private readonly ClienteValidator _validator = new();

    // Verifica que un cliente con datos válidos pase la validación.
    [Fact]
    public void Cliente_WithValidData_ReturnsTrue()
    {
        // Organizar (Arrange)
        var dto = new SaveClienteDto
        {
            Name = "Juan Perez",
            DocumentNumber = "12345678",
            Email = "juan.perez@example.com",
            Phone = "987654321",
            Address = "Av. Principal 123",
            Age = 30
        };

        // Actuar (Act)
        var result = _validator.Validate(dto);

        // Afirmar (Assert)
        Assert.True(result.IsValid);
    }

    // Verifica que un cliente sin documento o correo no sea válido.
    [Fact]
    public void Cliente_MissingRequiredData_ReturnsFalse()
    {
        // Organizar (Arrange)
        var dto = new SaveClienteDto
        {
            Name = "Empresa SAC",
            DocumentNumber = "",
            Email = "",
            Address = "Calle 1"
        };

        // Actuar (Act)
        var result = _validator.Validate(dto);

        // Afirmar (Assert)
        Assert.False(result.IsValid);
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
