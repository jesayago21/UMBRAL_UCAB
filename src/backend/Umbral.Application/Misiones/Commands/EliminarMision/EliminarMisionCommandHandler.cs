using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoMision.Mision;

namespace Umbral.Application.Misiones.Commands.EliminarMision;

internal sealed class EliminarMisionCommandHandler : IRequestHandler<EliminarMisionCommand, Result<Guid>>
{
    private readonly IMisionRepository _misionRepository;

    public EliminarMisionCommandHandler(IMisionRepository misionRepository)
    {
        _misionRepository = misionRepository;
    }

    public async Task<Result<Guid>> Handle(EliminarMisionCommand command, CancellationToken cancellationToken)
    {
        var misionId = new MisionId(command.MisionId);
        var mision = await _misionRepository.FindByIdAsync(misionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Mision), command.MisionId);

        await _misionRepository.DeleteAsync(mision, cancellationToken);
        return Result<Guid>.Ok(mision.MisionId.Valor);
    }
}
