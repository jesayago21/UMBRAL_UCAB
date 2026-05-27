using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.PausarSesion;

internal sealed class PausarSesionCommandHandler
    : IRequestHandler<PausarSesionCommand, Result<Guid>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;

    public PausarSesionCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository = sesionRepository;
        _eventPublisher   = eventPublisher;
    }

    public async Task<Result<Guid>> Handle(
        PausarSesionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        sesion.Pausar();

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        return Result<Guid>.Ok(sesion.SesionId.Valor);
    }
}
