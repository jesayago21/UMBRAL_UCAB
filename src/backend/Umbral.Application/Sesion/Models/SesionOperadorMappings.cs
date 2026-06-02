using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Models;

internal static class SesionOperadorMappings
{
    public static SesionResumenDto ToResumen(this SesionAR sesion)
    {
        var (misionId, misionNombre, totalEtapas, etapaOrden, etapaDescripcion, etapaTipo) =
            MapContexto(sesion);

        return new SesionResumenDto(
            sesion.SesionId.Valor,
            sesion.TipoSesion.ToString(),
            misionId,
            misionNombre,
            sesion.Estado.ToString(),
            sesion.Participantes.Count,
            IniciadaEnUtc(sesion),
            sesion.FinalizadaEn,
            etapaOrden,
            totalEtapas,
            etapaDescripcion,
            etapaTipo);
    }

    public static SesionDetalleDto ToDetalle(this SesionAR sesion)
    {
        var resumen = sesion.ToResumen();
        var participantes = sesion.Participantes
            .Select(e => new ParticipanteSesionDto(
                e.ParticipanteId.Valor,
                e.JugadorId.Valor,
                e.Nombre.Valor))
            .ToList();

        return new SesionDetalleDto(
            resumen.Id,
            resumen.TipoSesion,
            resumen.MisionId,
            resumen.MisionNombre,
            resumen.Estado,
            sesion.CodigoAcceso.Valor,
            resumen.IniciadaEn,
            resumen.FinalizadaEn,
            resumen.EtapaActualOrden,
            resumen.TotalEtapas,
            resumen.EtapaActualDescripcion,
            resumen.EtapaActivaTipo,
            participantes,
            MapEtapas(sesion));
    }

    private static (
        Guid MisionId,
        string Nombre,
        int Total,
        int Orden,
        string? Descripcion,
        string? EtapaTipo) MapContexto(SesionAR sesion)
    {
        if (sesion.ContextoMision is not null)
        {
            var ctx      = sesion.ContextoMision;
            var snapshot = ctx.MisionSnapshot;
            var total    = snapshot.Etapas.Count;
            var etapa    = total > 0 && ctx.EtapaActualIndex < total
                ? snapshot.Etapas[ctx.EtapaActualIndex]
                : null;

            var descripcion = etapa switch
            {
                EtapaBusquedaTesoroSnapshot bt => bt.Descripcion,
                EtapaTriviaSnapshot trivia =>
                    $"Trivia: {trivia.CategoriasTitulo}",
                _ => null
            };

            return (
                snapshot.MisionId.Valor,
                snapshot.Nombre,
                total,
                etapa?.Orden ?? 0,
                descripcion,
                etapa?.Tipo.ToString());
        }

        if (sesion.ContextoBT is not null)
        {
            var (id, nombre, total, orden, desc) = MapBusquedaTesoroLegacy(sesion.ContextoBT);
            return (id, nombre, total, orden, desc, TipoEtapa.BusquedaTesoro.ToString());
        }

        if (sesion.ContextoTrivia is not null)
        {
            var ctx = sesion.ContextoTrivia;
            return (
                Guid.Empty,
                ctx.CategoriasTitulo,
                ctx.TotalPreguntas,
                ctx.PreguntaActualIndex + 1,
                $"Pregunta {ctx.PreguntaActualIndex + 1} de {ctx.TotalPreguntas}",
                TipoEtapa.Trivia.ToString());
        }

        return (Guid.Empty, "Sesión", 0, 0, null, null);
    }

    private static IReadOnlyList<EtapaSesionDto>? MapEtapas(SesionAR sesion)
    {
        if (sesion.ContextoMision is null && sesion.ContextoBT is null)
            return null;

        var snapshot = sesion.ContextoMision?.MisionSnapshot
                       ?? sesion.ContextoBT!.MisionSnapshot;
        var index = sesion.ContextoMision?.EtapaActualIndex
                    ?? sesion.ContextoBT!.EtapaActualIndex;

        var etapaActualOrden = snapshot.Etapas.Count > 0 && index < snapshot.Etapas.Count
            ? snapshot.Etapas[index].Orden
            : 0;

        return snapshot.Etapas
            .OrderBy(e => e.Orden)
            .Select(e =>
            {
                if (e is EtapaBusquedaTesoroSnapshot bt)
                {
                    return new EtapaSesionDto(
                        bt.Orden,
                        bt.Tipo.ToString(),
                        bt.Descripcion,
                        bt.Orden == etapaActualOrden,
                        bt.Pistas.Select(p => new PistaSesionDto(
                            p.Contenido,
                            p.TipoLiberacion.ToString(),
                            p.SegundosLiberacion)).ToList(),
                        null);
                }

                var trivia = (EtapaTriviaSnapshot)e;
                return new EtapaSesionDto(
                    trivia.Orden,
                    trivia.Tipo.ToString(),
                    trivia.CategoriasTitulo,
                    trivia.Orden == etapaActualOrden,
                    null,
                    trivia.CategoriaIds.Select(c => c.Valor).ToList());
            })
            .ToList();
    }

    private static (Guid, string, int, int, string?) MapBusquedaTesoroLegacy(ContextoBusquedaTesoro contexto)
    {
        var snapshot    = contexto.MisionSnapshot;
        var totalEtapas = snapshot.Etapas.Count;
        var etapaIndex  = contexto.EtapaActualIndex;
        var etapa = totalEtapas > 0 && etapaIndex < totalEtapas
            ? snapshot.Etapas[etapaIndex] as EtapaBusquedaTesoroSnapshot
            : null;

        return (
            snapshot.MisionId.Valor,
            snapshot.Nombre,
            totalEtapas,
            etapa?.Orden ?? 0,
            etapa?.Descripcion);
    }

    private static DateTime? IniciadaEnUtc(SesionAR sesion) =>
        sesion.Estado is EstadoSesion.Programada or EstadoSesion.EnPreparacion
            ? null
            : sesion.IniciadaEn == default
                ? null
                : sesion.IniciadaEn;
}
