using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOs.Empresas;

public class SaveEmpresaDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El NIT es obligatorio")]
    [MaxLength(50)]
    public string Nit { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;
}