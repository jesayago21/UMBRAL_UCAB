using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;

internal sealed class CrearSesionBusquedaTesoroCommandHandler
    : IRequestHandler<CrearSesionBusquedaTesoroCommand, Result<CrearSesionBusquedaTesoroResult>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IMisionRepository _misionRepository;
    private readonly IEventPublisher _eventPublisher;

    public CrearSesionBusquedaTesoroCommandHandler(
        ISesionRepository sesionRepository,
        IMisionRepository misionRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository = sesionRepository;
        _misionRepository = misionRepository;
        _eventPublisher   = eventPublisher;
    }

    public async Task<Result<CrearSesionBusquedaTesoroResult>> Handle(
        CrearSesionBusquedaTesoroCommand command,
        CancellationToken cancellationToken)
    {
        var mision = await _misionRepository.FindByIdAsync(
                         new MisionId(command.MisionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(Mision), command.MisionId);

        if (!mision.PuedeUsarseParaSesion())
            throw new DomainException(
                "La misión debe estar activa para crear una sesión.");

        var snapshot = MisionSnapshot.Desde(mision);
        var sesion   = SesionAR.CrearBusquedaTesoro(
            snapshot,
            new UsuarioId(command.OperadorId));

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        return Result<CrearSesionBusquedaTesoroResult>.Ok(
            new CrearSesionBusquedaTesoroResult(
                sesion.SesionId.Valor,
                sesion.CodigoAcceso.Valor));
    }
}
