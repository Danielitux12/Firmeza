namespace Firmeza.Application.DTOs.Customers;

/// <summary>
/// Modelo de datos para consultar o responder información de un cliente.
/// </summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? UserId { get; set; }
}
