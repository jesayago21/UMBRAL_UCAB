using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.FinalizarSesion;

internal sealed class FinalizarSesionCommandHandler
    : IRequestHandler<FinalizarSesionCommand, Result<Guid>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public FinalizarSesionCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _eventPublisher       = eventPublisher;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<Guid>> Handle(
        FinalizarSesionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        sesion.Finalizar();

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        await _notificacionRealTime.NotificarCambioEstadoSesionAsync(
            sesion.SesionId.Valor.ToString(),
            sesion.Estado.ToString(),
            cancellationToken);

        await _notificacionRealTime.NotificarRankingActualizadoAsync(
            sesion.SesionId.Valor.ToString(),
            RankingNotificacionMapper.DesdeSesion(sesion),
            cancellationToken);

        return Result<Guid>.Ok(sesion.SesionId.Valor);
    }
}
