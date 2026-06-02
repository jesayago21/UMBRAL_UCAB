using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

public sealed class EtapaBusquedaTesoro : Etapa
{
    public string Descripcion { get; private set; } = default!;
    public string CodigoQRSolucion { get; private set; } = default!;

    private readonly List<Pista> _pistas = [];
    public IReadOnlyList<Pista> Pistas => _pistas.AsReadOnly();

    public override TipoEtapa Tipo => TipoEtapa.BusquedaTesoro;

    private EtapaBusquedaTesoro() { }

    internal static EtapaBusquedaTesoro Crear(
        MisionId misionId,
        int orden,
        string descripcion,
        string codigoQRSolucion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción de la etapa no puede estar vacía.");

        if (string.IsNullOrWhiteSpace(codigoQRSolucion))
            throw new DomainException("El código QR solución de la etapa no puede estar vacío.");

        return new EtapaBusquedaTesoro
        {
            EtapaId          = EtapaId.Nuevo(),
            MisionId         = misionId,
            Orden            = orden,
            Descripcion      = descripcion.Trim(),
            CodigoQRSolucion = codigoQRSolucion.Trim()
        };
    }

    public void AgregarPista(
        string contenido,
        TipoLiberacion tipoLiberacion,
        int? segundosLiberacion = null)
    {
        var pista = Pista.Crear(EtapaId, contenido, tipoLiberacion, segundosLiberacion);
        _pistas.Add(pista);
    }
}
