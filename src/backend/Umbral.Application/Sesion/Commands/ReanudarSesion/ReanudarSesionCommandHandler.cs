using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.ReanudarSesion;

internal sealed class ReanudarSesionCommandHandler
    : IRequestHandler<ReanudarSesionCommand, Result<Guid>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public ReanudarSesionCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _eventPublisher       = eventPublisher;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<Guid>> Handle(
        ReanudarSesionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        sesion.Reanudar();

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        await _notificacionRealTime.NotificarCambioEstadoSesionAsync(
            sesion.SesionId.Valor.ToString(),
            sesion.Estado.ToString(),
            cancellationToken);

        return Result<Guid>.Ok(sesion.SesionId.Valor);
    }
}
