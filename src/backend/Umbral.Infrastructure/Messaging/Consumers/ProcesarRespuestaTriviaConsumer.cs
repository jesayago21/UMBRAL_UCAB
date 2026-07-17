using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.ProcesarRespuestaTrivia;
using Umbral.Domain.Shared;
using Umbral.Infrastructure.Messaging.Contracts;

namespace Umbral.Infrastructure.Messaging.Consumers;

/// <summary>HU-35 — valida y persiste respuestas trivia fuera del hilo HTTP.</summary>
public sealed class ProcesarRespuestaTriviaConsumer
    : IConsumer<RespuestaTriviaRecibidaIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProcesarRespuestaTriviaConsumer> _logger;

    public ProcesarRespuestaTriviaConsumer(
        IMediator mediator,
        ILogger<ProcesarRespuestaTriviaConsumer> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    public async Task Consume(ConsumeContext<RespuestaTriviaRecibidaIntegrationEvent> context)
    {
        var msg = context.Message;

        if (!EsMensajeValido(msg, out var error))
        {
            _logger.LogWarning(
                "Mensaje trivia malformado descartado (HU-35): {Error} MessageId={MessageId}",
                error,
                msg.MessageId);
            return;
        }

        try
        {
            var result = await _mediator.Send(
                new ProcesarRespuestaTriviaCommand(
                    msg.SesionId,
                    msg.JugadorId,
                    msg.PreguntaId,
                    msg.IndiceOpcion,
                    msg.DuracionTimerSegundos,
                    msg.RecibidoEnUtc),
                context.CancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogInformation(
                    "Respuesta trivia rechazada en dominio MessageId={MessageId}: {Errores}",
                    msg.MessageId,
                    string.Join("; ", result.Errors));
            }
        }
        catch (DomainException ex)
        {
            _logger.LogInformation(
                ex,
                "Dominio rechazó respuesta trivia (idempotente/RB-12/RB-13) MessageId={MessageId}",
                msg.MessageId);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "Recurso no encontrado al procesar respuesta trivia MessageId={MessageId}",
                msg.MessageId);
        }
    }

    private static bool EsMensajeValido(
        RespuestaTriviaRecibidaIntegrationEvent msg,
        out string error)
    {
        if (msg.MessageId == Guid.Empty)
        {
            error = "MessageId vacío.";
            return false;
        }

        if (msg.SesionId == Guid.Empty)
        {
            error = "SesionId vacío.";
            return false;
        }

        if (msg.JugadorId == Guid.Empty)
        {
            error = "JugadorId vacío.";
            return false;
        }

        if (msg.PreguntaId == Guid.Empty)
        {
            error = "PreguntaId vacío.";
            return false;
        }

        if (msg.IndiceOpcion < 0)
        {
            error = "IndiceOpcion negativo.";
            return false;
        }

        if (msg.DuracionTimerSegundos <= 0)
        {
            error = "DuracionTimerSegundos inválida.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
