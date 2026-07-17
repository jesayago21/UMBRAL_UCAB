using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.SubmitRespuestaTrivia;

/// <summary>HU-34 — Participante confirma opción de la pregunta trivia activa.</summary>
public sealed record SubmitRespuestaTriviaCommand(
    Guid SesionId,
    Guid JugadorId,
    Guid PreguntaId,
    int IndiceOpcion,
    int? DuracionTimerSegundos = null) : IRequest<Result<SubmitRespuestaTriviaResult>>;

/// <summary>Ack inmediato tras encolar (HU-35). El resultado llega vía GET estado / SignalR.</summary>
public sealed record SubmitRespuestaTriviaResult(
    Guid MessageId,
    string Status);
