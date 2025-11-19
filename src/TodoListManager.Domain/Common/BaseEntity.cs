namespace TodoListManager.Domain.Common;

/// <summary>
/// Base entity for domain entities.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }
}
