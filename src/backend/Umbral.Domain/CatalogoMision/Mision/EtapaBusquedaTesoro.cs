using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

public sealed class EtapaBusquedaTesoro : Etapa
{
    public const int RadioMetrosMinimo = 10;
    public const int RadioMetrosMaximo = 2000;

    public string Descripcion { get; private set; } = default!;
    public string CodigoQRSolucion { get; private set; } = default!;
    public double? Latitud { get; private set; }
    public double? Longitud { get; private set; }
    public int? RadioMetros { get; private set; }

    private readonly List<Pista> _pistas = [];
    public IReadOnlyList<Pista> Pistas => _pistas.AsReadOnly();

    public override TipoEtapa Tipo => TipoEtapa.BusquedaTesoro;

    private EtapaBusquedaTesoro() { }

    internal static EtapaBusquedaTesoro Crear(
        MisionId misionId,
        int orden,
        string descripcion,
        string codigoQRSolucion,
        double? latitud = null,
        double? longitud = null,
        int? radioMetros = null)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción de la etapa no puede estar vacía.");

        if (string.IsNullOrWhiteSpace(codigoQRSolucion))
            throw new DomainException("El código QR solución de la etapa no puede estar vacío.");

        ValidarUbicacion(latitud, longitud, radioMetros);

        return new EtapaBusquedaTesoro
        {
            EtapaId          = EtapaId.Nuevo(),
            MisionId         = misionId,
            Orden            = orden,
            Descripcion      = descripcion.Trim(),
            CodigoQRSolucion = codigoQRSolucion.Trim(),
            Latitud          = latitud,
            Longitud         = longitud,
            RadioMetros      = radioMetros
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

    public void EditarPista(
        PistaId pistaId,
        string contenido,
        TipoLiberacion tipoLiberacion,
        int? segundosLiberacion = null)
    {
        var pista = _pistas.FirstOrDefault(p => p.PistaId == pistaId)
            ?? throw new DomainException("La pista indicada no pertenece a esta etapa.");

        pista.Actualizar(contenido, tipoLiberacion, segundosLiberacion);
    }

    public void EliminarPista(PistaId pistaId)
    {
        var pista = _pistas.FirstOrDefault(p => p.PistaId == pistaId)
            ?? throw new DomainException("La pista indicada no pertenece a esta etapa.");

        _pistas.Remove(pista);
    }

    public void Actualizar(
        string descripcion,
        string codigoQRSolucion,
        double? latitud = null,
        double? longitud = null,
        int? radioMetros = null)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción de la etapa no puede estar vacía.");

        if (string.IsNullOrWhiteSpace(codigoQRSolucion))
            throw new DomainException("El código QR solución de la etapa no puede estar vacío.");

        ValidarUbicacion(latitud, longitud, radioMetros);

        Descripcion = descripcion.Trim();
        CodigoQRSolucion = codigoQRSolucion.Trim();
        Latitud = latitud;
        Longitud = longitud;
        RadioMetros = radioMetros;
    }

    private static void ValidarUbicacion(double? latitud, double? longitud, int? radioMetros)
    {
        var alguno = latitud.HasValue || longitud.HasValue || radioMetros.HasValue;
        var todos = latitud.HasValue && longitud.HasValue && radioMetros.HasValue;

        if (alguno && !todos)
            throw new DomainException(
                "La ubicación del tesoro requiere latitud, longitud y radio de búsqueda juntos.");

        if (!todos)
            return;

        if (latitud is < -90 or > 90)
            throw new DomainException("La latitud debe estar entre -90 y 90.");

        if (longitud is < -180 or > 180)
            throw new DomainException("La longitud debe estar entre -180 y 180.");

        if (radioMetros is < RadioMetrosMinimo or > RadioMetrosMaximo)
            throw new DomainException(
                $"El radio de búsqueda debe estar entre {RadioMetrosMinimo} y {RadioMetrosMaximo} metros.");
    }
}
