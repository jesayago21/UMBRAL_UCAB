namespace Umbral.Domain.Shared;

public abstract class Entity
{
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetType() == other.GetType() && IdEquals(other);
    }

    protected abstract bool IdEquals(Entity other);

    public override int GetHashCode() => GetIdHashCode();

    protected abstract int GetIdHashCode();
}
