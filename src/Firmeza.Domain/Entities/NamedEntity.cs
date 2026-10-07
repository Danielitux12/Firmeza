namespace Firmeza.Domain.Entities;

public abstract class NamedEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
}