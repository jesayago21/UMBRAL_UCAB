namespace Umbral.Domain.Ports;

/// <summary>
/// HU-35 — publica respuestas trivia para procesamiento asíncrono (RabbitMQ / MassTransit).
/// </summary>
public sealed record RespuestaTriviaMensaje(
    Guid MessageId,
    Guid SesionId,
    Guid JugadorId,
    Guid PreguntaId,
    int IndiceOpcion,
    int DuracionTimerSegundos,
    DateTimeOffset RecibidoEnUtc);

public interface IRespuestaTriviaBus
{
    Task PublicarAsync(RespuestaTriviaMensaje mensaje, CancellationToken ct = default);
}
