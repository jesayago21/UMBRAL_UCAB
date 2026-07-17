namespace Umbral.Infrastructure.RealTime.Hubs;

public sealed record SesionEstadoPayload(Guid SesionId, string NuevoEstado);

public sealed record ParticipantesActualizadosPayload(
    Guid SesionId,
    int ParticipantesCount);

public sealed record PistaLiberadaPayload(
    Guid SesionId,
    Guid ParticipanteId,
    Guid PistaId,
    int EtapaIndex,
    string Contenido);

public sealed record EtapaAvanzadaPayload(
    Guid SesionId,
    int EtapaIndex,
    string TipoEtapa);

public sealed record RankingActualizadoPayload(
    Guid SesionId,
    IReadOnlyList<PosicionRankingHubDto> Ranking);

public sealed record PosicionRankingHubDto(
    int Posicion,
    Guid ParticipanteId,
    string NombreParticipante,
    int PuntajeTotal,
    long TiempoAcumuladoMs = 0);

public sealed record PenalizacionAplicadaPayload(
    Guid SesionId,
    Guid ParticipanteId,
    int Puntos,
    string Motivo);

public sealed record ParticipanteExpulsadoPayload(
    Guid SesionId,
    Guid ParticipanteId,
    string Motivo);

public sealed record PreguntaTriviaIniciadaPayload(
    Guid SesionId,
    Guid PreguntaId,
    int Orden,
    string Enunciado,
    IReadOnlyList<string> Opciones,
    DateTime TimerCerradoEnUtc,
    int TotalPreguntas);

public sealed record TriviaEnTransicionPayload(
    Guid SesionId,
    int OrdenPreguntaCerrada,
    int TotalPreguntas,
    string Mensaje);

/// <summary>
/// Métodos que el servidor invoca en los clientes conectados al SesionHub (HU-15 / HU-17 / HU-32 / HU-33).
/// </summary>
public interface ISesionHubClient
{
    Task SesionEstadoCambiado(SesionEstadoPayload payload);

    Task ParticipantesActualizados(ParticipantesActualizadosPayload payload);

    Task PistaLiberada(PistaLiberadaPayload payload);

    Task EtapaAvanzada(EtapaAvanzadaPayload payload);

    Task RankingActualizado(RankingActualizadoPayload payload);

    Task PenalizacionAplicada(PenalizacionAplicadaPayload payload);

    Task ParticipanteExpulsado(ParticipanteExpulsadoPayload payload);

    Task PreguntaTriviaIniciada(PreguntaTriviaIniciadaPayload payload);

    Task TriviaEnTransicion(TriviaEnTransicionPayload payload);
}
