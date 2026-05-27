namespace Umbral.Domain.Sesion;

public sealed record EquipoId(Guid Valor)
{
    public static EquipoId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
