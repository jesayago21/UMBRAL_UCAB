using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

public sealed class PistaSnapshot : ValueObject
{
    public PistaId PistaId { get; }
    public string Contenido { get; }
    public TipoLiberacion TipoLiberacion { get; }
    public int? SegundosLiberacion { get; }

    private PistaSnapshot(PistaId pistaId, string contenido, TipoLiberacion tipo, int? segundos)
    {
        PistaId            = pistaId;
        Contenido          = contenido;
        TipoLiberacion     = tipo;
        SegundosLiberacion = segundos;
    }

    public static PistaSnapshot Desde(Pista pista) =>
        new(pista.PistaId, pista.Contenido, pista.TipoLiberacion, pista.SegundosLiberacion);

    internal static PistaSnapshot Rehydrate(
        PistaId pistaId,
        string contenido,
        TipoLiberacion tipo,
        int? segundos) =>
        new(pistaId, contenido, tipo, segundos);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PistaId;
        yield return Contenido;
        yield return TipoLiberacion;
        yield return SegundosLiberacion ?? 0;
    }
}
