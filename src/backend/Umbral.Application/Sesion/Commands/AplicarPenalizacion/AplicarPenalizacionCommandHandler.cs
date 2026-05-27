using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.AplicarPenalizacion;

internal sealed class AplicarPenalizacionCommandHandler
    : IRequestHandler<AplicarPenalizacionCommand, Result<Guid>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;

    public AplicarPenalizacionCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository = sesionRepository;
        _eventPublisher = eventPublisher;
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

        sesion.AplicarPenalizacion(new EquipoId(command.EquipoId), penalizacion);

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        return Result<Guid>.Ok(sesion.SesionId.Valor);
    }
}
