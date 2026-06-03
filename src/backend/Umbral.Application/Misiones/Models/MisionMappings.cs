using Umbral.Domain.CatalogoMision.Mision;

namespace Umbral.Application.Misiones.Models;

internal static class MisionMappings
{
    public static MisionDto ToDto(this Mision mision)
    {
        var etapas = mision.Etapas
            .OrderBy(x => x.Orden)
            .Select(MapEtapa)
            .ToList();

        return new MisionDto(
            mision.MisionId.Valor,
            mision.Nombre,
            mision.Estado == EstadoMision.Activa ? "Activa" : "Inactiva",
            etapas.Count,
            etapas);
    }

    private static EtapaMisionDto MapEtapa(Etapa etapa) => etapa switch
    {
        EtapaBusquedaTesoro bt => new EtapaMisionDto(
            bt.EtapaId.Valor,
            bt.Orden,
            TipoEtapa.BusquedaTesoro.ToString(),
            bt.Descripcion,
            bt.CodigoQRSolucion,
            bt.Pistas.Select(p => new PistaMisionDto(
                p.PistaId.Valor,
                p.Contenido,
                p.TipoLiberacion.ToString(),
                p.SegundosLiberacion)).ToList(),
            null),
        EtapaTrivia trivia => new EtapaMisionDto(
            trivia.EtapaId.Valor,
            trivia.Orden,
            TipoEtapa.Trivia.ToString(),
            null,
            null,
            null,
            trivia.CategoriaIds.Select(c => c.Valor).ToList()),
        _ => throw new InvalidOperationException($"Etapa no soportada: {etapa.GetType().Name}")
    };
}
