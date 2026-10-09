using Firmeza.Application.DTOs.Empresas;
using Firmeza.Application.Validators;
using Xunit;

namespace Firmeza.Tests;

public class EmpresaTests
{
    private readonly EmpresaValidator _validator = new();

    [Fact]
    public void Empresa_WithValidData_ReturnsValid()
    {
        var dto = new SaveEmpresaDto
        {
            Name = "Soluciones Empresariales S.A.S",
            Nit = "900123456-1",
            Email = "contacto@soluciones.com",
            Phone = "+57 300 123 4567",
            Address = "Carrera 7 # 100 - 20",
            IsActive = true
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Empresa_WithFormattedNitDots_ReturnsValid()
    {
        var dto = new SaveEmpresaDto
        {
            Name = "Distribuidora Nacional Ltda",
            Nit = "900.123.456-1",
            Email = "ventas@distribuidora.co",
            Phone = "6012345678",
            Address = "Calle 45 # 12-34 Oficina 501",
            IsActive = true
        };

        var result = _validator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Empresa_WithAlphabeticNit_ReturnsInvalid()
    {
        // Caso reportado por el usuario: NIT 'GDSVXZ' con letras debe ser rechazado
        var dto = new SaveEmpresaDto
        {
            Name = "dasdcxzc",
            Nit = "GDSVXZ",
            Email = "a@gmail.com",
            Phone = "3003552166",
            Address = "asfacasc",
            IsActive = true
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nit");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("ABCDEF")]
    [InlineData("900-ABC-1")]
    [InlineData("900123456-123")]
    public void Empresa_WithInvalidNit_FailsValidation(string invalidNit)
    {
        var dto = new SaveEmpresaDto
        {
            Name = "Empresa de Prueba",
            Nit = invalidNit,
            Email = "test@empresa.com",
            Phone = "3001234567",
            Address = "Calle Principal 123"
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nit");
    }

    [Theory]
    [InlineData("")]
    [InlineData("sin-arroba.com")]
    [InlineData("correo@")]
    [InlineData("@dominio.com")]
    public void Empresa_WithInvalidEmail_FailsValidation(string invalidEmail)
    {
        var dto = new SaveEmpresaDto
        {
            Name = "Empresa de Prueba",
            Nit = "900123456-1",
            Email = invalidEmail,
            Phone = "3001234567",
            Address = "Calle Principal 123"
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("telefono_invalido")]
    public void Empresa_WithInvalidPhone_FailsValidation(string invalidPhone)
    {
        var dto = new SaveEmpresaDto
        {
            Name = "Empresa de Prueba",
            Nit = "900123456-1",
            Email = "contacto@empresa.com",
            Phone = invalidPhone,
            Address = "Calle Principal 123"
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Phone");
    }

    [Fact]
    public void Empresa_WithShortAddress_FailsValidation()
    {
        var dto = new SaveEmpresaDto
        {
            Name = "Empresa de Prueba",
            Nit = "900123456-1",
            Email = "contacto@empresa.com",
            Phone = "3001234567",
            Address = "Abc" // Menor a 5 caracteres
        };

        var result = _validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Address");
    }
}
