using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Domain.CatalogoMision.Mision;

public sealed class EtapaBusquedaTesoroSnapshot : EtapaSnapshotBase
{
    public string Descripcion { get; }
    public string CodigoQRSolucion { get; }
    public IReadOnlyList<PistaSnapshot> Pistas { get; }

    public override TipoEtapa Tipo => TipoEtapa.BusquedaTesoro;

    private EtapaBusquedaTesoroSnapshot(
        EtapaId id,
        int orden,
        string descripcion,
        string codigoQR,
        IReadOnlyList<PistaSnapshot> pistas)
        : base(id, orden)
    {
        Descripcion      = descripcion;
        CodigoQRSolucion = codigoQR;
        Pistas           = pistas;
    }

    public static EtapaBusquedaTesoroSnapshot Desde(EtapaBusquedaTesoro etapa)
    {
        var pistas = etapa.Pistas
            .Select(PistaSnapshot.Desde)
            .ToList()
            .AsReadOnly();

        return new EtapaBusquedaTesoroSnapshot(
            etapa.EtapaId,
            etapa.Orden,
            etapa.Descripcion,
            etapa.CodigoQRSolucion,
            pistas);
    }

    internal static EtapaBusquedaTesoroSnapshot Rehydrate(
        EtapaId id,
        int orden,
        string descripcion,
        string codigoQr,
        IReadOnlyList<PistaSnapshot>? pistas = null) =>
        new(id, orden, descripcion, codigoQr, pistas ?? []);
}
