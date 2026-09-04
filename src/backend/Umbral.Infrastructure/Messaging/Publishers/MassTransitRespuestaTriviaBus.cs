using MassTransit;
using Umbral.Domain.Ports;
using Umbral.Infrastructure.Messaging.Contracts;

namespace Umbral.Infrastructure.Messaging.Publishers;

internal sealed class MassTransitRespuestaTriviaBus : IRespuestaTriviaBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitRespuestaTriviaBus(IPublishEndpoint publishEndpoint) =>
        _publishEndpoint = publishEndpoint;

    public Task PublicarAsync(RespuestaTriviaMensaje mensaje, CancellationToken ct = default) =>
        _publishEndpoint.Publish(
            new RespuestaTriviaRecibidaIntegrationEvent
            {
                MessageId             = mensaje.MessageId,
                SesionId              = mensaje.SesionId,
                JugadorId             = mensaje.JugadorId,
                PreguntaId            = mensaje.PreguntaId,
                IndiceOpcion          = mensaje.IndiceOpcion,
                DuracionTimerSegundos = mensaje.DuracionTimerSegundos,
                RecibidoEnUtc         = mensaje.RecibidoEnUtc
            },
            ct);
}
