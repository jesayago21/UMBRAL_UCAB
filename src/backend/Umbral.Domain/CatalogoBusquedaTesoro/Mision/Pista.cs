using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

public sealed class Pista : Entity
{
    public PistaId PistaId { get; private set; } = default!;
    public EtapaId EtapaId { get; private set; } = default!;
    public string Contenido { get; private set; } = default!;
    public TipoLiberacion TipoLiberacion { get; private set; }
    public int? SegundosLiberacion { get; private set; }

    private Pista() { }

    internal static Pista Crear(
        EtapaId etapaId,
        string contenido,
        TipoLiberacion tipo,
        int? segundos)
    {
        if (tipo == TipoLiberacion.PorTiempo && (segundos is null or <= 0))
            throw new DomainException(
                "El tipo PorTiempo requiere un valor de segundos de liberación mayor a cero.");

        if (string.IsNullOrWhiteSpace(contenido))
            throw new DomainException("El contenido de la pista no puede estar vacío.");

        return new Pista
        {
            PistaId            = PistaId.Nuevo(),
            EtapaId            = etapaId,
            Contenido          = contenido.Trim(),
            TipoLiberacion     = tipo,
            SegundosLiberacion = segundos
        };
    }

    protected override bool IdEquals(Entity other) =>
        other is Pista p && p.PistaId == PistaId;

    protected override int GetIdHashCode() => PistaId.GetHashCode();
}
