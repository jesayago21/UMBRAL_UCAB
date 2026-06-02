namespace Umbral.Domain.CatalogoMision.Mision;

public sealed record EtapaId(Guid Valor)
{
    public static EtapaId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
