using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Umbral.Domain.Ports;
using Umbral.Infrastructure.RealTime.Hubs;

namespace Umbral.Infrastructure.RealTime;

/// <summary>
/// Adaptador SignalR de <see cref="INotificacionRealTime"/> (HU-15 / HU-17).
/// </summary>
public sealed class NotificacionRealTimeService : INotificacionRealTime
{
    private readonly IHubContext<SesionHub, ISesionHubClient> _sesionHub;
    private readonly ILogger<NotificacionRealTimeService> _logger;

    public NotificacionRealTimeService(
        IHubContext<SesionHub, ISesionHubClient> sesionHub,
        ILogger<NotificacionRealTimeService> logger)
    {
        _sesionHub = sesionHub;
        _logger    = logger;
    }

    public async Task NotificarCambioEstadoSesionAsync(
        string sesionId,
        string nuevoEstado,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(sesionId, out var id))
            return;

        _logger.LogInformation(
            "SignalR SesionEstadoCambiado → grupo sesion-{SesionId} estado={Estado}",
            id,
            nuevoEstado);

        await _sesionHub.Clients
            .Group($"sesion-{id}")
            .SesionEstadoCambiado(new SesionEstadoPayload(id, nuevoEstado));
    }

    public async Task NotificarParticipantesActualizadosAsync(
        string sesionId,
        int participantesCount,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(sesionId, out var id))
            return;

        _logger.LogInformation(
            "SignalR ParticipantesActualizados → grupo sesion-{SesionId} count={Count}",
            id,
            participantesCount);

        await _sesionHub.Clients
            .Group($"sesion-{id}")
            .ParticipantesActualizados(new ParticipantesActualizadosPayload(id, participantesCount));
    }

    public async Task NotificarPistaLiberadaAsync(
        string sesionId,
        string participanteId,
        string pistaId,
        int etapaIndex,
        string contenido,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(sesionId, out var sid)
            || !Guid.TryParse(participanteId, out var pid)
            || !Guid.TryParse(pistaId, out var pista))
            return;

        var payload = new PistaLiberadaPayload(sid, pid, pista, etapaIndex, contenido);

        _logger.LogInformation(
            "SignalR PistaLiberada → grupo equipo-{ParticipanteId} pista={PistaId}",
            pid,
            pista);

        await _sesionHub.Clients
            .Group($"equipo-{pid}")
            .PistaLiberada(payload);
    }

    public async Task NotificarEtapaAvanzadaAsync(
        string sesionId,
        int etapaIndex,
        string tipoEtapa,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(sesionId, out var id))
            return;

        _logger.LogInformation(
            "SignalR EtapaAvanzada → grupo sesion-{SesionId} index={Index} tipo={Tipo}",
            id,
            etapaIndex,
            tipoEtapa);

        await _sesionHub.Clients
            .Group($"sesion-{id}")
            .EtapaAvanzada(new EtapaAvanzadaPayload(id, etapaIndex, tipoEtapa));
    }

    public async Task NotificarRankingActualizadoAsync(
        string sesionId,
        IReadOnlyList<RankingPosicionNotificacion> ranking,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(sesionId, out var id))
            return;

        var posiciones = ranking
            .Select(r => new PosicionRankingHubDto(
                r.Posicion,
                r.ParticipanteId,
                r.NombreParticipante,
                r.PuntajeTotal,
                r.TiempoAcumuladoMs))
            .ToList();

        var payload = new RankingActualizadoPayload(id, posiciones);
        var grupo = $"sesion-{id}";

        _logger.LogInformation(
            "SignalR RankingActualizado → grupo {Grupo} posiciones={Count}",
            grupo,
            posiciones.Count);

        await _sesionHub.Clients.Group(grupo).RankingActualizado(payload);
        await _sesionHub.Clients.Group($"operador-{id}").RankingActualizado(payload);
    }

    public async Task NotificarPenalizacionAplicadaAsync(
        string sesionId,
        string participanteId,
        int puntos,
        string motivo,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(sesionId, out var sid)
            || !Guid.TryParse(participanteId, out var pid))
            return;

        var payload = new PenalizacionAplicadaPayload(sid, pid, puntos, motivo ?? string.Empty);

        _logger.LogInformation(
            "SignalR PenalizacionAplicada → equipo-{ParticipanteId} puntos={Puntos}",
            pid,
            puntos);

        await _sesionHub.Clients
            .Group($"equipo-{pid}")
            .PenalizacionAplicada(payload);
    }

    public async Task NotificarParticipanteExpulsadoAsync(
        string sesionId,
        string participanteId,
        string motivo,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(sesionId, out var sid)
            || !Guid.TryParse(participanteId, out var pid))
            return;

        var payload = new ParticipanteExpulsadoPayload(sid, pid, motivo ?? string.Empty);

        _logger.LogInformation(
            "SignalR ParticipanteExpulsado → equipo-{ParticipanteId}",
            pid);

        await _sesionHub.Clients
            .Group($"equipo-{pid}")
            .ParticipanteExpulsado(payload);
    }

    public async Task NotificarPreguntaTriviaIniciadaAsync(
        string sesionId,
        string preguntaId,
        int orden,
        string enunciado,
        IReadOnlyList<string> opciones,
        DateTime timerCerradoEnUtc,
        int totalPreguntas,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(sesionId, out var sid)
            || !Guid.TryParse(preguntaId, out var pid))
            return;

        var payload = new PreguntaTriviaIniciadaPayload(
            sid,
            pid,
            orden,
            enunciado ?? string.Empty,
            opciones ?? Array.Empty<string>(),
            timerCerradoEnUtc,
            totalPreguntas);

        _logger.LogInformation(
            "SignalR PreguntaTriviaIniciada → sesion-{SesionId} orden={Orden}",
            sid,
            orden);

        await _sesionHub.Clients
            .Group($"sesion-{sid}")
            .PreguntaTriviaIniciada(payload);
    }

    public async Task NotificarTriviaEnTransicionAsync(
        string sesionId,
        int ordenPreguntaCerrada,
        int totalPreguntas,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(sesionId, out var sid))
            return;

        var payload = new TriviaEnTransicionPayload(
            sid,
            ordenPreguntaCerrada,
            totalPreguntas,
            "Preparando siguiente pregunta…");

        _logger.LogInformation(
            "SignalR TriviaEnTransicion → sesion-{SesionId}",
            sid);

        await _sesionHub.Clients
            .Group($"sesion-{sid}")
            .TriviaEnTransicion(payload);
    }
}
