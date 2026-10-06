namespace Firmeza.Domain.Entities;

public class Empresa : ContactableEntity
{
    public string Nit { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Nit)
            && HasValidEmail();
    }
}