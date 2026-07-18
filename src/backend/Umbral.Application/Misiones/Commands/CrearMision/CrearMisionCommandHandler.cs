using MediatR;
using Umbral.Application.Common.Models;
using Umbral.Application.Misiones.Services;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;

namespace Umbral.Application.Misiones.Commands.CrearMision;

internal sealed class CrearMisionCommandHandler : IRequestHandler<CrearMisionCommand, Result<Guid>>
{
    private readonly IMisionRepository _misionRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IPreguntaRepository _preguntaRepository;

    public CrearMisionCommandHandler(
        IMisionRepository misionRepository,
        ICategoriaRepository categoriaRepository,
        IPreguntaRepository preguntaRepository)
    {
        _misionRepository = misionRepository;
        _categoriaRepository = categoriaRepository;
        _preguntaRepository = preguntaRepository;
    }

    public async Task<Result<Guid>> Handle(CrearMisionCommand command, CancellationToken cancellationToken)
    {
        var nombre = command.Nombre.Trim();
        var exists = await _misionRepository.ExistsByNombreAsync(nombre, ct: cancellationToken);
        if (exists)
            throw new DomainException($"Ya existe una misión con el nombre '{nombre}'.");

        var mision = Mision.Crear(command.Nombre);

        foreach (var etapa in command.Etapas.OrderBy(e => e.Orden))
        {
            if (string.Equals(etapa.TipoEtapa, "Trivia", StringComparison.OrdinalIgnoreCase))
            {
                var cats = (etapa.CategoriaIds ?? [])
                    .Select(id => new CategoriaId(id))
                    .ToList();
                await MisionTriviaValidacion.AsegurarCategoriasExistenAsync(
                    cats, _categoriaRepository, cancellationToken);
                mision.AgregarEtapaTrivia(cats);
            }
            else
            {
                mision.AgregarEtapaBusquedaTesoro(
                    etapa.Descripcion ?? string.Empty,
                    etapa.CodigoQrSolucion ?? string.Empty,
                    etapa.Latitud,
                    etapa.Longitud,
                    etapa.RadioMetros);

                // TODO E2 (O3): el AR debería devolver EtapaId al agregar, en lugar de navegar .Last().
                var etapaBt = mision.Etapas.OfType<EtapaBusquedaTesoro>().Last();
                foreach (var pista in etapa.Pistas ?? [])
                {
                    var tipo = TipoLiberacionParser.Parse(pista.TipoLiberacion);
                    etapaBt.AgregarPista(pista.Contenido, tipo, pista.SegundosLiberacion);
                }
            }
        }

        if (command.Activar)
        {
            await MisionTriviaValidacion.AsegurarPreguntasActivasParaActivacionAsync(
                mision, _preguntaRepository, _categoriaRepository, cancellationToken);
            mision.Activar();
        }

        await _misionRepository.SaveAsync(mision, cancellationToken);
        return Result<Guid>.Ok(mision.MisionId.Valor);
    }
}
