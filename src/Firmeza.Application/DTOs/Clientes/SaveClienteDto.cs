using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Clientes;

/// <summary>
/// Modelo de datos para crear o actualizar un cliente vía API.
/// </summary>
public class SaveClienteDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de documento es obligatorio")]
    [MaxLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Range(0, 120, ErrorMessage = "La edad debe estar entre 0 y 120 años")]
    public int Age { get; set; }

    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;
}
