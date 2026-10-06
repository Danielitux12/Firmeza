namespace Firmeza.Application.DTOs.Auth;

/// <summary>
/// Modelo de datos para solicitud de inicio de sesión en la API.
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
