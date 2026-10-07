namespace Firmeza.Domain.Entities;

public class Cliente : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string? UserId { get; set; }
}