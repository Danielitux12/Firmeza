namespace Firmeza.Application.DTOs.Auth;

/// <summary>
/// Modelo de datos para registrar un nuevo cliente desde la API.
/// </summary>
public class RegisterRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
}
