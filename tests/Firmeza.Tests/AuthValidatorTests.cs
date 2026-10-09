using Firmeza.Application.DTOs.Auth;
using Firmeza.Application.Validators;
using Xunit;

namespace Firmeza.Tests;

public class AuthValidatorTests
{
    private readonly LoginRequestValidator _loginValidator = new();
    private readonly RegisterRequestValidator _registerValidator = new();

    [Fact]
    public void LoginRequest_WithValidData_ReturnsTrue()
    {
        var dto = new LoginRequestDto
        {
            Email = "usuario@firmeza.com",
            Password = "Password123*"
        };

        var result = _loginValidator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Password123*")]
    [InlineData("invalid-email", "Password123*")]
    [InlineData("usuario@firmeza.com", "")]
    [InlineData("usuario@firmeza.com", "123")]
    public void LoginRequest_WithInvalidData_ReturnsFalse(string email, string password)
    {
        var dto = new LoginRequestDto
        {
            Email = email,
            Password = password
        };

        var result = _loginValidator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void RegisterRequest_WithValidData_ReturnsTrue()
    {
        var dto = new RegisterRequestDto
        {
            Name = "Carlos Rodriguez",
            DocumentNumber = "1020304050",
            Email = "carlos@example.com",
            Password = "SecurePassword123*",
            Phone = "3001234567",
            Age = 25,
            Address = "Carrera 15 # 45 - 20"
        };

        var result = _registerValidator.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "1020304050", "carlos@example.com", "Pass123", 25)] // missing name
    [InlineData("Carlos", "123", "carlos@example.com", "Pass123", 25)] // short doc
    [InlineData("Carlos", "1020304050", "invalid-email", "Pass123", 25)] // invalid email
    [InlineData("Carlos", "1020304050", "carlos@example.com", "123", 25)] // short password
    [InlineData("Carlos", "1020304050", "carlos@example.com", "Pass123", 16)] // under 18
    public void RegisterRequest_WithInvalidData_ReturnsFalse(string name, string doc, string email, string password, int age)
    {
        var dto = new RegisterRequestDto
        {
            Name = name,
            DocumentNumber = doc,
            Email = email,
            Password = password,
            Phone = "3001234567",
            Age = age,
            Address = "Calle 1 # 2 - 3"
        };

        var result = _registerValidator.Validate(dto);

        Assert.False(result.IsValid);
    }
}
