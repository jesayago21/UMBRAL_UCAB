namespace Umbral.Domain.Sesion;

public sealed class SesionId : IEquatable<SesionId>
{
    public Guid Value { get; }

    public SesionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                "El identificador de sesión no puede ser vacío.", nameof(value));
        Value = value;
    }

    public static SesionId Nuevo() => new(Guid.NewGuid());

    public bool Equals(SesionId? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) =>
        obj is SesionId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();
}
