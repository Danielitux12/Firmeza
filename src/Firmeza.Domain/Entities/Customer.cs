namespace Firmeza.Domain.Entities;

/// <summary>
/// Representa a un cliente registrado en el sistema.
/// </summary>
public class Customer
{
    // Identificador único del cliente.
    public int Id { get; set; }

    // Nombre completo o razón social del cliente.
    public string FullName { get; set; } = string.Empty;

    // Documento de identidad (DNI, RUC, CC) - Debe ser único.
    public string DocumentNumber { get; set; } = string.Empty;

    // Correo electrónico de contacto - Debe ser único.
    public string Email { get; set; } = string.Empty;

    // Teléfono de contacto.
    public string Phone { get; set; } = string.Empty;

    // Fecha de nacimiento del cliente.
    public DateTime? BirthDate { get; set; }

    // Dirección fiscal o de entrega.
    public string Address { get; set; } = string.Empty;

    // Identificador opcional del usuario en ASP.NET Core Identity (para clientes con cuenta de acceso).
    public string? UserId { get; set; }

    // Historial de compras/ventas realizadas por este cliente.
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    // Valida que el cliente contenga la información mínima obligatoria.
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(FullName)
            && !string.IsNullOrWhiteSpace(DocumentNumber)
            && !string.IsNullOrWhiteSpace(Email);
    }
}
