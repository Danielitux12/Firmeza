namespace Firmeza.Application.DTOs.Clientes;

public class ClienteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Role { get; set; } = "Cliente";
    public bool IsActive { get; set; }
}