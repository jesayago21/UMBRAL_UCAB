using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Application.Misiones.Commands.EditarPistaEtapa;

internal sealed class EditarPistaEtapaCommandHandler
    : IRequestHandler<EditarPistaEtapaCommand, Result<Guid>>
{
    private readonly IMisionRepository _misionRepository;

    public EditarPistaEtapaCommandHandler(IMisionRepository misionRepository) =>
        _misionRepository = misionRepository;

    public async Task<Result<Guid>> Handle(
        EditarPistaEtapaCommand command,
        CancellationToken cancellationToken)
    {
        var misionId = new MisionId(command.MisionId);
        var mision = await _misionRepository.FindByIdAsync(misionId, cancellationToken)
            ?? throw new NotFoundException("Misión", command.MisionId);

        if (await _misionRepository.HasSesionesActivasAsync(misionId, cancellationToken))
            throw new DomainException(
                "No se puede editar pistas de una misión con sesiones activas (RB-11).");

        var tipo = Enum.Parse<TipoLiberacion>(command.TipoLiberacion, ignoreCase: true);
        mision.EditarPistaEtapa(
            new EtapaId(command.EtapaId),
            new PistaId(command.PistaId),
            command.Contenido,
            tipo,
            command.SegundosLiberacion);

        await _misionRepository.SaveAsync(mision, cancellationToken);
        return Result<Guid>.Ok(command.PistaId);
    }
}
