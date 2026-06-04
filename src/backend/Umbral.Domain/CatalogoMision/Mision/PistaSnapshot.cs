using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

public sealed class PistaSnapshot : ValueObject
{
    public string Contenido { get; }
    public TipoLiberacion TipoLiberacion { get; }
    public int? SegundosLiberacion { get; }

    private PistaSnapshot(string contenido, TipoLiberacion tipo, int? segundos)
    {
        Contenido          = contenido;
        TipoLiberacion     = tipo;
        SegundosLiberacion = segundos;
    }

    public static PistaSnapshot Desde(Pista pista) =>
        new(pista.Contenido, pista.TipoLiberacion, pista.SegundosLiberacion);

    internal static PistaSnapshot Rehydrate(
        string contenido,
        TipoLiberacion tipo,
        int? segundos) =>
        new(contenido, tipo, segundos);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Contenido;
        yield return TipoLiberacion;
        yield return SegundosLiberacion ?? 0;
    }
}
