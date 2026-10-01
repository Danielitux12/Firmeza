namespace Firmeza.Domain.Entities;

public class Employee
{
    // Identificador principal del empleado.
    public int Id { get; set; }

    // Nombre del empleado.
    public string FirstName { get; set; } = string.Empty;

    // Apellido del empleado.
    public string LastName { get; set; } = string.Empty;

    // Número de documento para identificación.
    public string DocumentNumber { get; set; } = string.Empty;

    // Correo electrónico del empleado.
    public string Email { get; set; } = string.Empty;

    // Teléfono opcional.
    public string? Phone { get; set; }

    // Cargo o posición del empleado.
    public string Position { get; set; } = string.Empty;

    // Salario actual del empleado.
    public decimal Salary { get; set; }

    // Indica si el empleado está activo.
    public bool IsActive { get; set; } = true;

    // Activa el empleado.
    public void Activate()
    {
        IsActive = true;
    }

    // Desactiva el empleado.
    public void Deactivate()
    {
        IsActive = false;
    }

    // Valida que los datos básicos del empleado sean correctos.
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(FirstName)
            && !string.IsNullOrWhiteSpace(LastName)
            && !string.IsNullOrWhiteSpace(DocumentNumber)
            && !string.IsNullOrWhiteSpace(Email)
            && Salary >= 0;
    }
}