using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Models;

internal static class SesionOperadorMappings
{
    public static SesionResumenDto ToResumen(this SesionAR sesion)
    {
        var (misionId, misionNombre, totalEtapas, etapaOrden, etapaDescripcion) =
            sesion.TipoSesion switch
            {
                TipoSesion.Trivia when sesion.ContextoTrivia is not null =>
                    MapTrivia(sesion.ContextoTrivia),
                _ when sesion.ContextoBT is not null =>
                    MapBusquedaTesoro(sesion.ContextoBT),
                _ => (Guid.Empty, "Sesión", 0, 0, (string?)null)
            };

        return new SesionResumenDto(
            sesion.SesionId.Valor,
            sesion.TipoSesion.ToString(),
            misionId,
            misionNombre,
            sesion.Estado.ToString(),
            sesion.Equipos.Count,
            IniciadaEnUtc(sesion),
            sesion.FinalizadaEn,
            etapaOrden,
            totalEtapas,
            etapaDescripcion);
    }

    public static SesionDetalleDto ToDetalle(this SesionAR sesion)
    {
        var resumen = sesion.ToResumen();
        var equipos = sesion.Equipos
            .Select(e => new EquipoSesionDto(
                e.EquipoId.Valor,
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
            equipos);
    }

    private static (Guid MisionId, string Nombre, int Total, int Orden, string? Descripcion)
        MapBusquedaTesoro(ContextoBusquedaTesoro contexto)
    {
        var snapshot = contexto.MisionSnapshot;
        var totalEtapas = snapshot.Etapas.Count;
        var etapaIndex = contexto.EtapaActualIndex;
        var etapa = totalEtapas > 0 && etapaIndex < totalEtapas
            ? snapshot.Etapas[etapaIndex]
            : null;

        return (
            snapshot.MisionId.Valor,
            snapshot.Nombre,
            totalEtapas,
            etapa?.Orden ?? 0,
            etapa?.Descripcion);
    }

    private static (Guid MisionId, string Nombre, int Total, int Orden, string? Descripcion)
        MapTrivia(ContextoTrivia contexto)
    {
        var total = contexto.TotalPreguntas;
        var index = contexto.PreguntaActualIndex;
        var orden = total > 0 ? index + 1 : 0;
        var descripcion = total > 0
            ? $"Pregunta {orden} de {total}"
            : null;

        return (
            Guid.Empty,
            $"Trivia ({total} pregunta{(total == 1 ? "" : "s")})",
            total,
            orden,
            descripcion);
    }

    private static DateTime? IniciadaEnUtc(SesionAR sesion) =>
        sesion.Estado is EstadoSesion.Programada or EstadoSesion.EnPreparacion
            ? null
            : sesion.IniciadaEn == default
                ? null
                : sesion.IniciadaEn;
}
