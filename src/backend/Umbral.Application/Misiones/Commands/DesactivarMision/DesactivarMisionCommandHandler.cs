using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Application.Misiones.Commands.DesactivarMision;

internal sealed class DesactivarMisionCommandHandler : IRequestHandler<DesactivarMisionCommand, Result<Guid>>
{
    private readonly IMisionRepository _misionRepository;

    public DesactivarMisionCommandHandler(IMisionRepository misionRepository)
    {
        _misionRepository = misionRepository;
    }

    public async Task<Result<Guid>> Handle(DesactivarMisionCommand command, CancellationToken cancellationToken)
    {
        var mision = await _misionRepository.FindByIdAsync(new MisionId(command.MisionId), cancellationToken)
            ?? throw new NotFoundException(nameof(Mision), command.MisionId);

        mision.Desactivar();
        await _misionRepository.SaveAsync(mision, cancellationToken);

        return Result<Guid>.Ok(mision.MisionId.Valor);
    }
}
