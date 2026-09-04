using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Application.Misiones.Services;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;

namespace Umbral.Application.Misiones.Commands.EditarEtapaMision;

internal sealed class EditarEtapaMisionCommandHandler
    : IRequestHandler<EditarEtapaMisionCommand, Result<Guid>>
{
    private readonly IMisionRepository _misionRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public EditarEtapaMisionCommandHandler(
        IMisionRepository misionRepository,
        ICategoriaRepository categoriaRepository)
    {
        _misionRepository = misionRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Result<Guid>> Handle(
        EditarEtapaMisionCommand command,
        CancellationToken cancellationToken)
    {
        var misionId = new MisionId(command.MisionId);
        var mision = await _misionRepository.FindByIdAsync(misionId, cancellationToken)
            ?? throw new NotFoundException("Misión", command.MisionId);

        if (await _misionRepository.HasSesionesActivasAsync(misionId, cancellationToken))
            throw new DomainException(
                "No se puede editar etapas de una misión con sesiones activas (RB-11).");

        var etapaId = new EtapaId(command.EtapaId);
        var etapa = mision.Etapas.FirstOrDefault(e => e.EtapaId == etapaId)
            ?? throw new NotFoundException("Etapa", command.EtapaId);

        if (etapa is EtapaBusquedaTesoro)
        {
            mision.EditarEtapaBusquedaTesoro(
                etapaId,
                command.Descripcion ?? string.Empty,
                command.CodigoQrSolucion ?? string.Empty,
                command.Latitud,
                command.Longitud,
                command.RadioMetros);
        }
        else if (etapa is EtapaTrivia)
        {
            var cats = (command.CategoriaIds ?? [])
                .Select(id => new CategoriaId(id))
                .ToList();
            await MisionTriviaValidacion.AsegurarCategoriasExistenAsync(
                cats, _categoriaRepository, cancellationToken);
            mision.EditarEtapaTrivia(etapaId, cats);
        }
        else
        {
            throw new DomainException("Tipo de etapa no soportado para edición.");
        }

        await _misionRepository.SaveAsync(mision, cancellationToken);
        return Result<Guid>.Ok(command.EtapaId);
    }
}
