namespace Firmeza.Application.DTOs.Trabajadores;

public class TrabajadorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}