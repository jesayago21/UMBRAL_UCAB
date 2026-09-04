using Umbral.Domain.Ports;

namespace Umbral.Infrastructure.Messaging.Publishers;

/// <summary>
/// Fallback NoOp (tests / entornos sin SignalR host).
/// </summary>
public sealed class NoOpNotificacionRealTime : INotificacionRealTime
{
    public Task NotificarCambioEstadoSesionAsync(
        string sesionId,
        string nuevoEstado,
        CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotificarParticipantesActualizadosAsync(
        string sesionId,
        int participantesCount,
        CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotificarPistaLiberadaAsync(
        string sesionId,
        string participanteId,
        string pistaId,
        int etapaIndex,
        string contenido,
        CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotificarEtapaAvanzadaAsync(
        string sesionId,
        int etapaIndex,
        string tipoEtapa,
        CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotificarRankingActualizadoAsync(
        string sesionId,
        IReadOnlyList<RankingPosicionNotificacion> ranking,
        CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotificarPenalizacionAplicadaAsync(
        string sesionId,
        string participanteId,
        int puntos,
        string motivo,
        CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotificarParticipanteExpulsadoAsync(
        string sesionId,
        string participanteId,
        string motivo,
        CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotificarPreguntaTriviaIniciadaAsync(
        string sesionId,
        string preguntaId,
        int orden,
        string enunciado,
        IReadOnlyList<string> opciones,
        DateTime timerCerradoEnUtc,
        int totalPreguntas,
        CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotificarTriviaEnTransicionAsync(
        string sesionId,
        int ordenPreguntaCerrada,
        int totalPreguntas,
        CancellationToken ct = default)
        => Task.CompletedTask;
}
