namespace Umbral.Domain.Ports;

/// <summary>Snapshot de posición para push SignalR (HU-21 / HU-39).</summary>
public sealed record RankingPosicionNotificacion(
    Guid ParticipanteId,
    string NombreParticipante,
    int PuntajeTotal,
    int Posicion,
    long TiempoAcumuladoMs = 0);

public interface INotificacionRealTime
{
    Task NotificarCambioEstadoSesionAsync(
        string sesionId,
        string nuevoEstado,
        CancellationToken ct = default);

    /// <summary>
    /// Alguien se unió o abandonó: operador debe refrescar lista de participantes (HU-13 / lobby).
    /// </summary>
    Task NotificarParticipantesActualizadosAsync(
        string sesionId,
        int participantesCount,
        CancellationToken ct = default);

    Task NotificarPistaLiberadaAsync(
        string sesionId,
        string participanteId,
        string pistaId,
        int etapaIndex,
        string contenido,
        CancellationToken ct = default);

    /// <summary>
    /// HU-17 — avance de etapa tras evidencia válida (o fin de misión). El cliente refetch REST.
    /// </summary>
    Task NotificarEtapaAvanzadaAsync(
        string sesionId,
        int etapaIndex,
        string tipoEtapa,
        CancellationToken ct = default);

    /// <summary>
    /// HU-21 — ranking completo empujado a la sesión (sin depender de F5 / botón refrescar).
    /// </summary>
    Task NotificarRankingActualizadoAsync(
        string sesionId,
        IReadOnlyList<RankingPosicionNotificacion> ranking,
        CancellationToken ct = default);

    /// <summary>
    /// HU-16 — notifica al participante afectado el motivo de la penalización (grupo equipo).
    /// </summary>
    Task NotificarPenalizacionAplicadaAsync(
        string sesionId,
        string participanteId,
        int puntos,
        string motivo,
        CancellationToken ct = default);

    /// <summary>
    /// HU-32 — el operador expulsó al participante de la sala (grupo equipo).
    /// </summary>
    Task NotificarParticipanteExpulsadoAsync(
        string sesionId,
        string participanteId,
        string motivo,
        CancellationToken ct = default);

    /// <summary>HU-33 — pregunta trivia abierta con timer absoluto (grupo sesión).</summary>
    Task NotificarPreguntaTriviaIniciadaAsync(
        string sesionId,
        string preguntaId,
        int orden,
        string enunciado,
        IReadOnlyList<string> opciones,
        DateTime timerCerradoEnUtc,
        int totalPreguntas,
        CancellationToken ct = default);

    /// <summary>HU-33 — transición entre preguntas (grupo sesión).</summary>
    Task NotificarTriviaEnTransicionAsync(
        string sesionId,
        int ordenPreguntaCerrada,
        int totalPreguntas,
        CancellationToken ct = default);
}
