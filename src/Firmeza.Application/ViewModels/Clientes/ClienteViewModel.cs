using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.ViewModels.Clientes;

/// <summary>
/// Modelo de vista para creación y edición de clientes.
/// El campo Edad se recibe como texto según requerimiento para demostrar manejo de excepciones.
/// </summary>
public class ClienteViewModel
{
    // Identificador único del cliente (0 al crear).
    public int Id { get; set; }

    // Nombre del cliente.
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    // Documento de identidad (DNI/RUC/Cédula).
    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "El documento debe tener entre 5 y 50 caracteres.")]
    [Display(Name = "Número de Documento")]
    public string DocumentNumber { get; set; } = string.Empty;

    // Correo electrónico único.
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    // Teléfono de contacto.
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9+\-\s]{7,20}$", ErrorMessage = "Ingresa un número de teléfono válido (solo dígitos y guiones).")]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    // Campo Edad recibido como texto plano para validación con try-catch.
    [Required(ErrorMessage = "La edad es obligatoria.")]
    [Display(Name = "Edad (años)")]
    public string AgeText { get; set; } = string.Empty;

    // Dirección física.
    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(250, ErrorMessage = "La dirección no puede exceder 250 caracteres.")]
    [Display(Name = "Dirección")]
    public string Address { get; set; } = string.Empty;

    // Edad numérica procesada (para visualización).
    public int? ProcessedAge { get; set; }
}
