using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.AbandonarSesion;

internal sealed class AbandonarSesionCommandHandler
    : IRequestHandler<AbandonarSesionCommand, Result<Unit>>
{
    private readonly ISesionRepository _sesionRepository;

    public AbandonarSesionCommandHandler(ISesionRepository sesionRepository)
        => _sesionRepository = sesionRepository;

    public async Task<Result<Unit>> Handle(
        AbandonarSesionCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var participanteId = sesion.AbandonarParticipante(new UsuarioId(command.JugadorId));

        await _sesionRepository.EliminarParticipanteAsync(participanteId, cancellationToken);
        await _sesionRepository.SaveAsync(sesion, cancellationToken);

        return Result<Unit>.Ok(Unit.Value);
    }
}
