using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.ProcesarRespuestaTrivia;

/// <summary>HU-35 — procesamiento interno (consumer RabbitMQ).</summary>
public sealed record ProcesarRespuestaTriviaCommand(
    Guid SesionId,
    Guid JugadorId,
    Guid PreguntaId,
    int IndiceOpcion,
    int DuracionTimerSegundos,
    DateTimeOffset RecibidoEnUtc) : IRequest<Result<ProcesarRespuestaTriviaResult>>;

public sealed record ProcesarRespuestaTriviaResult(
    Guid RespuestaId,
    bool EsCorrecta,
    bool FueraDeTiempo,
    int PuntosOtorgados,
    int PuntajeTotal);
