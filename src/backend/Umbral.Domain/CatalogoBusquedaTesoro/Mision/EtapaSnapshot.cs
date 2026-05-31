using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

public sealed class EtapaSnapshot : ValueObject
{
    public EtapaId EtapaId { get; }
    public int Orden { get; }
    public string Descripcion { get; }
    public string CodigoQRSolucion { get; }
    public IReadOnlyList<PistaSnapshot> Pistas { get; }

    private EtapaSnapshot(
        EtapaId id,
        int orden,
        string descripcion,
        string codigoQR,
        IReadOnlyList<PistaSnapshot> pistas)
    {
        EtapaId          = id;
        Orden            = orden;
        Descripcion      = descripcion;
        CodigoQRSolucion = codigoQR;
        Pistas           = pistas;
    }

    public static EtapaSnapshot Desde(Etapa etapa)
    {
        var pistas = etapa.Pistas
            .Select(PistaSnapshot.Desde)
            .ToList()
            .AsReadOnly();

        return new EtapaSnapshot(
            etapa.EtapaId,
            etapa.Orden,
            etapa.Descripcion,
            etapa.CodigoQRSolucion,
            pistas);
    }

    /// <summary>Reconstitución desde persistencia (no usar en lógica de negocio).</summary>
    internal static EtapaSnapshot Rehydrate(
        EtapaId id,
        int orden,
        string descripcion,
        string codigoQr,
        IReadOnlyList<PistaSnapshot>? pistas = null) =>
        new(id, orden, descripcion, codigoQr, pistas ?? []);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EtapaId;
        yield return Orden;
    }
}
