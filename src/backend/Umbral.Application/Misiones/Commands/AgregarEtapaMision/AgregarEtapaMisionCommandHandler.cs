using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Application.Misiones.Services;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;

namespace Umbral.Application.Misiones.Commands.AgregarEtapaMision;

internal sealed class AgregarEtapaMisionCommandHandler
    : IRequestHandler<AgregarEtapaMisionCommand, Result<Guid>>
{
    private readonly IMisionRepository _misionRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public AgregarEtapaMisionCommandHandler(
        IMisionRepository misionRepository,
        ICategoriaRepository categoriaRepository)
    {
        _misionRepository = misionRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Result<Guid>> Handle(
        AgregarEtapaMisionCommand command,
        CancellationToken cancellationToken)
    {
        var misionId = new MisionId(command.MisionId);
        var mision = await _misionRepository.FindByIdAsync(misionId, cancellationToken)
            ?? throw new NotFoundException("Misión", command.MisionId);

        if (await _misionRepository.HasSesionesActivasAsync(misionId, cancellationToken))
            throw new DomainException(
                "No se puede agregar etapas a una misión con sesiones activas (RB-11).");

        if (string.Equals(command.TipoEtapa, "Trivia", StringComparison.OrdinalIgnoreCase))
        {
            var cats = (command.CategoriaIds ?? [])
                .Select(id => new CategoriaId(id))
                .ToList();
            await MisionTriviaValidacion.AsegurarCategoriasExistenAsync(
                cats, _categoriaRepository, cancellationToken);
            mision.AgregarEtapaTrivia(cats);
        }
        else
        {
            mision.AgregarEtapaBusquedaTesoro(
                command.Descripcion ?? string.Empty,
                command.CodigoQrSolucion ?? string.Empty,
                command.Latitud,
                command.Longitud,
                command.RadioMetros);

            var etapaBt = mision.Etapas.OfType<EtapaBusquedaTesoro>().Last();
            foreach (var pista in command.Pistas ?? [])
            {
                var tipo = Enum.Parse<TipoLiberacion>(pista.TipoLiberacion, ignoreCase: true);
                etapaBt.AgregarPista(pista.Contenido, tipo, pista.SegundosLiberacion);
            }
        }

        var nueva = mision.Etapas[^1];
        await _misionRepository.SaveAsync(mision, cancellationToken);
        return Result<Guid>.Ok(nueva.EtapaId.Valor);
    }
}
