using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.AplicarPenalizacion;

internal sealed class AplicarPenalizacionCommandHandler
    : IRequestHandler<AplicarPenalizacionCommand, Result<Guid>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public AplicarPenalizacionCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _eventPublisher       = eventPublisher;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<Guid>> Handle(
        AplicarPenalizacionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var penalizacion = new Penalizacion(
            command.Puntos,
            command.Motivo,
            new UsuarioId(command.OperadorId));

        sesion.AplicarPenalizacion(new ParticipanteId(command.ParticipanteId), penalizacion);

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        var sesionId = sesion.SesionId.Valor.ToString();
        var participanteId = command.ParticipanteId.ToString();

        await _notificacionRealTime.NotificarRankingActualizadoAsync(
            sesionId,
            RankingNotificacionMapper.DesdeSesion(sesion),
            cancellationToken);

        await _notificacionRealTime.NotificarPenalizacionAplicadaAsync(
            sesionId,
            participanteId,
            penalizacion.Puntos,
            penalizacion.Motivo,
            cancellationToken);

        return Result<Guid>.Ok(sesion.SesionId.Valor);
    }
}
