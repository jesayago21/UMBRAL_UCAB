using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Application.Misiones.Commands.ActualizarMision;

internal sealed class ActualizarMisionCommandHandler : IRequestHandler<ActualizarMisionCommand, Result<Guid>>
{
    private readonly IMisionRepository _misionRepository;

    public ActualizarMisionCommandHandler(IMisionRepository misionRepository)
    {
        _misionRepository = misionRepository;
    }

    public async Task<Result<Guid>> Handle(ActualizarMisionCommand command, CancellationToken cancellationToken)
    {
        var mision = await _misionRepository.FindByIdAsync(new MisionId(command.MisionId), cancellationToken)
            ?? throw new NotFoundException(nameof(Mision), command.MisionId);

        var nombre = command.Nombre.Trim();
        var exists = await _misionRepository.ExistsByNombreAsync(
            nombre,
            command.MisionId,
            cancellationToken);
        if (exists)
            throw new DomainException($"Ya existe una misión con el nombre '{nombre}'.");

        var hasActivas = await _misionRepository.HasSesionesActivasAsync(mision.MisionId, cancellationToken);
        if (hasActivas)
            throw new DomainException("No se puede editar una misión con sesiones activas.");

        mision.Renombrar(command.Nombre);

        if (command.Activar.HasValue)
        {
            if (command.Activar.Value && mision.Estado != EstadoMision.Activa)
                mision.Activar();
            else if (!command.Activar.Value && mision.Estado == EstadoMision.Activa)
                mision.Desactivar();
        }

        await _misionRepository.SaveAsync(mision, cancellationToken);
        return Result<Guid>.Ok(mision.MisionId.Valor);
    }
}
