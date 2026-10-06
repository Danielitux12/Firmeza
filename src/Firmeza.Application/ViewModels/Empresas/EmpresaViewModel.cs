using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.ViewModels.Empresas;

public class EmpresaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [StringLength(150, MinimumLength = 2)]
    [Display(Name = "Razón social")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El NIT es obligatorio.")]
    [StringLength(50, MinimumLength = 5)]
    [Display(Name = "NIT")]
    public string Nit { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Dirección")]
    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}