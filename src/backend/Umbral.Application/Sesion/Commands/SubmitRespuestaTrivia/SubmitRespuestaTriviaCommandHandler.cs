using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Application.Sesion.Commands.ProcesarRespuestaTrivia;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.SubmitRespuestaTrivia;

/// <summary>
/// HU-34/35 — valida inscripción, encola mensaje y responde de inmediato (202 Accepted).
/// </summary>
internal sealed class SubmitRespuestaTriviaCommandHandler
    : IRequestHandler<SubmitRespuestaTriviaCommand, Result<SubmitRespuestaTriviaResult>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IRespuestaTriviaBus _respuestaTriviaBus;

    public SubmitRespuestaTriviaCommandHandler(
        ISesionRepository sesionRepository,
        IRespuestaTriviaBus respuestaTriviaBus)
    {
        _sesionRepository   = sesionRepository;
        _respuestaTriviaBus = respuestaTriviaBus;
    }

    public async Task<Result<SubmitRespuestaTriviaResult>> Handle(
        SubmitRespuestaTriviaCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var jugadorId = new UsuarioId(command.JugadorId);
        _ = sesion.Participantes.FirstOrDefault(p => p.JugadorId == jugadorId)
            ?? throw new DomainException("No estás inscrito en esta sesión.");

        var recibidoEn = DateTimeOffset.UtcNow;
        var messageId  = Guid.NewGuid();
        var duracion   = command.DuracionTimerSegundos
                         ?? ProcesarRespuestaTriviaCommandHandler.DuracionPorDefectoSegundos;

        await _respuestaTriviaBus.PublicarAsync(
            new RespuestaTriviaMensaje(
                messageId,
                command.SesionId,
                command.JugadorId,
                command.PreguntaId,
                command.IndiceOpcion,
                duracion,
                recibidoEn),
            cancellationToken);

        return Result<SubmitRespuestaTriviaResult>.Ok(
            new SubmitRespuestaTriviaResult(messageId, "Accepted"));
    }
}
