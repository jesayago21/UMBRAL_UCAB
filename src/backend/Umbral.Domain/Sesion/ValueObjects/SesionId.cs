namespace Umbral.Domain.Sesion;

public sealed record SesionId(Guid Valor)
{
    public static SesionId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
