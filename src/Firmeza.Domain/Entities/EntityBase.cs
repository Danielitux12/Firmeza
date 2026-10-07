namespace Firmeza.Domain.Entities;

public abstract class EntityBase
{
    public int Id { get; set; }

    public bool IsActive { get; private set; } = true;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}