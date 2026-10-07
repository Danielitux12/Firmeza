namespace Firmeza.Domain.Entities;

public class Trabajador : ContactableEntity
{
    public string DocumentNumber { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(DocumentNumber)
            && !string.IsNullOrWhiteSpace(Position)
            && Salary >= 0
            && HasValidEmail();
    }
}