using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.SubmitEvidencia;

internal sealed class SubmitEvidenciaCommandHandler
    : IRequestHandler<SubmitEvidenciaCommand, Result<SubmitEvidenciaResult>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;

    public SubmitEvidenciaCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher)
    {
        _sesionRepository = sesionRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<SubmitEvidenciaResult>> Handle(
        SubmitEvidenciaCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var evidencia = sesion.RegistrarEvidencia(
            new EquipoId(command.EquipoId),
            command.CodigoQr);

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        return Result<SubmitEvidenciaResult>.Ok(
            new SubmitEvidenciaResult(
                evidencia.EvidenciaId.Valor,
                evidencia.Resultado));
    }
}
