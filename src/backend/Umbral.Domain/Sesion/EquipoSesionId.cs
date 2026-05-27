namespace Umbral.Domain.Sesion;

public sealed class EquipoSesionId : IEquatable<EquipoSesionId>
{
    public Guid Value { get; }

    public EquipoSesionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                "El identificador de equipo no puede ser vacío.", nameof(value));
        Value = value;
    }

    public static EquipoSesionId Nuevo() => new(Guid.NewGuid());

    public bool Equals(EquipoSesionId? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) =>
        obj is EquipoSesionId other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();
}
