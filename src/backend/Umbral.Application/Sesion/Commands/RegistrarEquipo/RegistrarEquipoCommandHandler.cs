using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.RegistrarEquipo;

internal sealed class RegistrarEquipoCommandHandler
    : IRequestHandler<RegistrarEquipoCommand, Result<RegistrarEquipoResult>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;

    public RegistrarEquipoCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository = sesionRepository;
        _eventPublisher   = eventPublisher;
    }

    public async Task<Result<RegistrarEquipoResult>> Handle(
        RegistrarEquipoCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        if (sesion.Estado == EstadoSesion.Programada)
            sesion.AbrirParaRegistro();

        var equipo = sesion.RegistrarEquipo(command.NombreEquipo);

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        return Result<RegistrarEquipoResult>.Ok(
            new RegistrarEquipoResult(
                equipo.EquipoId.Valor,
                equipo.CodigoAcceso.Valor));
    }
}
