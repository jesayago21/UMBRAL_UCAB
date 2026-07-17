using MediatR;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;

namespace Umbral.Application.Sesion.Commands.LiberarPistasPorTiempoVencidas;

internal sealed class LiberarPistasPorTiempoVencidasCommandHandler
    : IRequestHandler<LiberarPistasPorTiempoVencidasCommand, Result<int>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public LiberarPistasPorTiempoVencidasCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _eventPublisher       = eventPublisher;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<int>> Handle(
        LiberarPistasPorTiempoVencidasCommand command,
        CancellationToken cancellationToken)
    {
        var ahora = command.Ahora ?? DateTimeOffset.UtcNow;
        var sesiones = await _sesionRepository.FindActivasAsync(cancellationToken);
        var total = 0;

        foreach (var sesion in sesiones)
        {
            var liberadas = sesion.LiberarPistasPorTiempoVencidas(ahora);
            if (liberadas == 0)
                continue;

            total += liberadas;

            await _sesionRepository.SaveAsync(sesion, cancellationToken);

            var eventosPista = sesion.DomainEvents.OfType<PistaLiberada>().ToList();

            await _eventPublisher.PublishBatchAsync(sesion.DomainEvents, cancellationToken);
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
        }

        return Result<int>.Ok(total);
    }
}
