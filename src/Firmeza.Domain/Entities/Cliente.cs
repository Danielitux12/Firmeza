namespace Firmeza.Domain.Entities;

public class Cliente : ContactableEntity
{
    public string DocumentNumber { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public string? UserId { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(DocumentNumber)
            && HasValidEmail();
    }
}