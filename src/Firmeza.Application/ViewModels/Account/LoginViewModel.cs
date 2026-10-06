using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.ViewModels.Account;

/// <summary>
/// Modelo de vista para el formulario de inicio de sesión de administradores.
/// </summary>
public class LoginViewModel
{
    // Correo electrónico del usuario.
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    // Contraseña de acceso.
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    // Indica si se debe recordar la sesión en el navegador.
    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }
}
