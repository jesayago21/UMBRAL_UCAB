using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion;

/// <summary>Mapeo compartido del ranking de dominio → notificación SignalR (HU-21 / HU-39).</summary>
internal static class RankingNotificacionMapper
{
    public static IReadOnlyList<RankingPosicionNotificacion> DesdeSesion(SesionAR sesion) =>
        RankingService.Calcular(sesion.Participantes, sesion.RespuestasTrivia)
            .Select(p => new RankingPosicionNotificacion(
                p.ParticipanteId.Valor,
                p.NombreParticipante,
                p.PuntajeTotal,
                p.Posicion,
                p.TiempoAcumuladoMs))
            .ToList();
}
