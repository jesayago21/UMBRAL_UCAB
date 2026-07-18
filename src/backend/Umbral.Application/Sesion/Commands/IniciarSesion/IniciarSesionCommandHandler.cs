using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.IniciarSesion;

internal sealed class IniciarSesionCommandHandler
    : IRequestHandler<IniciarSesionCommand, Result<Guid>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public IniciarSesionCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _eventPublisher       = eventPublisher;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<Guid>> Handle(
        IniciarSesionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        sesion.Iniciar();

        var eventosPista = sesion.DomainEvents.OfType<PistaLiberada>().ToList();

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        var sesionId = sesion.SesionId.Valor.ToString();

        await _notificacionRealTime.NotificarCambioEstadoSesionAsync(
            sesionId,
            sesion.Estado.ToString(),
            cancellationToken);

        foreach (var evt in eventosPista)
        {
            await _notificacionRealTime.NotificarPistaLiberadaAsync(
                sesionId,
                evt.ParticipanteId.Valor.ToString(),
                evt.PistaId.Valor.ToString(),
                evt.EtapaIndex,
                evt.Contenido,
                cancellationToken);
        }

        return Result<Guid>.Ok(sesion.SesionId.Valor);
    }
}
