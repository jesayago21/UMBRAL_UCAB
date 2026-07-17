using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.LiberarPistaManual;

internal sealed class LiberarPistaManualCommandHandler
    : IRequestHandler<LiberarPistaManualCommand, Result<int>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public LiberarPistaManualCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _eventPublisher       = eventPublisher;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<int>> Handle(
        LiberarPistaManualCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        ParticipanteId? destinatario = command.ParticipanteId is { } id
            ? new ParticipanteId(id)
            : null;

        var liberadas = sesion.LiberarPistaManual(
            command.Contenido,
            destinatario,
            DateTimeOffset.UtcNow);

        await _sesionRepository.SaveAsync(sesion, cancellationToken);

        var eventosPista = sesion.DomainEvents.OfType<PistaLiberada>().ToList();

        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        foreach (var evt in eventosPista)
        {
            await _notificacionRealTime.NotificarPistaLiberadaAsync(
                evt.SesionId.Valor.ToString(),
                evt.ParticipanteId.Valor.ToString(),
                evt.PistaId.Valor.ToString(),
                evt.EtapaIndex,
                evt.Contenido,
                cancellationToken);
        }

        return Result<int>.Ok(liberadas);
    }
}
