namespace Firmeza.Application.DTOs.Auth;

/// <summary>
/// Modelo de respuesta con el token JWT devuelto al iniciar sesión.
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
