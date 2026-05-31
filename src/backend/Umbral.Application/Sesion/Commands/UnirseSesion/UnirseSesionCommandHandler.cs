using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.UnirseSesion;

internal sealed class UnirseSesionCommandHandler
    : IRequestHandler<UnirseSesionCommand, Result<UnirseSesionResult>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;

    public UnirseSesionCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository = sesionRepository;
        _eventPublisher   = eventPublisher;
    }

    public async Task<Result<UnirseSesionResult>> Handle(
        UnirseSesionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var equipo = sesion.UnirseEquipo(
            new UsuarioId(command.JugadorId),
            command.NombreEquipo,
            command.CodigoAcceso);

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        return Result<UnirseSesionResult>.Ok(new UnirseSesionResult(equipo.EquipoId.Valor));
    }
}
