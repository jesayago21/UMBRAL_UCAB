namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

public sealed record PistaId(Guid Valor)
{
    public static PistaId Nuevo() => new(Guid.NewGuid());
    public override string ToString() => Valor.ToString();
}
