using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Application.Sesion;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.AbandonarSesion;

internal sealed class AbandonarSesionCommandHandler
    : IRequestHandler<AbandonarSesionCommand, Result<Unit>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly INotificacionRealTime _notificacionRealTime;

    public AbandonarSesionCommandHandler(
        ISesionRepository sesionRepository,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<Unit>> Handle(
        AbandonarSesionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var jugadorId = new UsuarioId(command.JugadorId);
        var esPostPartida = sesion.Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada;

        var participanteId = sesion.AbandonarParticipante(jugadorId);

        await _sesionRepository.EliminarParticipanteAsync(participanteId, cancellationToken);
        await _sesionRepository.SaveAsync(sesion, cancellationToken);

        // Tras partidas viejas el jugador podía seguir en varias Finalizadas: al salir limpia todas.
        if (esPostPartida)
        {
            await _sesionRepository.EliminarParticipacionesEnSesionesTerminalesAsync(
                jugadorId,
                cancellationToken);
        }

        var sesionId = sesion.SesionId.Valor.ToString();

        await _notificacionRealTime.NotificarParticipantesActualizadosAsync(
            sesionId,
            sesion.Participantes.Count,
            cancellationToken);

        await _notificacionRealTime.NotificarRankingActualizadoAsync(
            sesionId,
            RankingNotificacionMapper.DesdeSesion(sesion),
            cancellationToken);

        return Result<Unit>.Ok(Unit.Value);
    }
}
