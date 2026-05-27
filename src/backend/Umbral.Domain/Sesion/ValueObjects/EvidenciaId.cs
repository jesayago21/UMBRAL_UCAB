namespace Umbral.Domain.Sesion;

public sealed record EvidenciaId(Guid Valor)
{
    public static EvidenciaId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
