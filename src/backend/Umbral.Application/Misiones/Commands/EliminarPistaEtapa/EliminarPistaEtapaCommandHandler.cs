using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Application.Misiones.Commands.EliminarPistaEtapa;

internal sealed class EliminarPistaEtapaCommandHandler
    : IRequestHandler<EliminarPistaEtapaCommand, Result<Unit>>
{
    private readonly IMisionRepository _misionRepository;

    public EliminarPistaEtapaCommandHandler(IMisionRepository misionRepository) =>
        _misionRepository = misionRepository;

    public async Task<Result<Unit>> Handle(
        EliminarPistaEtapaCommand command,
        CancellationToken cancellationToken)
    {
        var misionId = new MisionId(command.MisionId);
        var mision = await _misionRepository.FindByIdAsync(misionId, cancellationToken)
            ?? throw new NotFoundException("Misión", command.MisionId);

        if (await _misionRepository.HasSesionesActivasAsync(misionId, cancellationToken))
            throw new DomainException(
                "No se puede eliminar pistas de una misión con sesiones activas (RB-11).");

        mision.EliminarPistaEtapa(
            new EtapaId(command.EtapaId),
            new PistaId(command.PistaId));

        await _misionRepository.SaveAsync(mision, cancellationToken);
        return Result<Unit>.Ok(Unit.Value);
    }
}
