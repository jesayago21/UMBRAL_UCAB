using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Application.Sesion;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.ExpulsarParticipante;

internal sealed class ExpulsarParticipanteCommandHandler
    : IRequestHandler<ExpulsarParticipanteCommand, Result<Unit>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly INotificacionRealTime _notificacionRealTime;

    public ExpulsarParticipanteCommandHandler(
        ISesionRepository sesionRepository,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<Unit>> Handle(
        ExpulsarParticipanteCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var participanteId = sesion.ExpulsarParticipante(
            new ParticipanteId(command.ParticipanteId),
            command.Motivo);

        await _sesionRepository.EliminarParticipanteAsync(participanteId, cancellationToken);
        await _sesionRepository.SaveAsync(sesion, cancellationToken);

        var sesionId = sesion.SesionId.Valor.ToString();
        var participanteIdStr = participanteId.Valor.ToString();

        await _notificacionRealTime.NotificarParticipantesActualizadosAsync(
            sesionId,
            sesion.Participantes.Count,
            cancellationToken);

        await _notificacionRealTime.NotificarRankingActualizadoAsync(
            sesionId,
            RankingNotificacionMapper.DesdeSesion(sesion),
            cancellationToken);

        await _notificacionRealTime.NotificarParticipanteExpulsadoAsync(
            sesionId,
            participanteIdStr,
            command.Motivo.Trim(),
            cancellationToken);

        return Result<Unit>.Ok(Unit.Value);
    }
}
