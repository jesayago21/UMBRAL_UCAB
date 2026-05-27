namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

public sealed record MisionId(Guid Valor)
{
    public static MisionId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
