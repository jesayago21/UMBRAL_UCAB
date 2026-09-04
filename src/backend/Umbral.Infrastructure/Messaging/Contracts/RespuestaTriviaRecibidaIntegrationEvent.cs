namespace Umbral.Infrastructure.Messaging.Contracts;

/// <summary>HU-35 — contrato de integración para respuestas trivia (RabbitMQ).</summary>
public sealed record RespuestaTriviaRecibidaIntegrationEvent
{
    public Guid MessageId { get; init; }
    public Guid SesionId { get; init; }
    public Guid JugadorId { get; init; }
    public Guid PreguntaId { get; init; }
    public int IndiceOpcion { get; init; }
    public int DuracionTimerSegundos { get; init; }
    public DateTimeOffset RecibidoEnUtc { get; init; }
}
