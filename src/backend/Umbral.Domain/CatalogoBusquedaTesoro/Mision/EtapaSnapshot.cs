using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

public sealed class EtapaSnapshot : ValueObject
{
    public EtapaId EtapaId { get; }
    public int Orden { get; }
    public string Descripcion { get; }
    public string CodigoQRSolucion { get; }

    private EtapaSnapshot(EtapaId id, int orden, string descripcion, string codigoQR)
    {
        EtapaId          = id;
        Orden            = orden;
        Descripcion      = descripcion;
        CodigoQRSolucion = codigoQR;
    }

    public static EtapaSnapshot Desde(Etapa etapa) =>
        new(etapa.EtapaId, etapa.Orden, etapa.Descripcion, etapa.CodigoQRSolucion);

    /// <summary>Reconstitución desde persistencia (no usar en lógica de negocio).</summary>
    internal static EtapaSnapshot Rehydrate(
        EtapaId id,
        int orden,
        string descripcion,
        string codigoQr) =>
        new(id, orden, descripcion, codigoQr);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EtapaId;
        yield return Orden;
    }
}
