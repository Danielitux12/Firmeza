using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Trabajadores;

public class SaveTrabajadorDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal Salary { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;
}