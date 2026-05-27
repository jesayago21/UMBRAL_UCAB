namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

public sealed record EtapaId(Guid Valor)
{
    public static EtapaId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
