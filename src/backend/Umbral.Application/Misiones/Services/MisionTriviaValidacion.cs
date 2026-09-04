using Umbral.Application.Common.Exceptions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;

namespace Umbral.Application.Misiones.Services;

/// <summary>
/// Validaciones de Application para etapas Trivia (HU-40 / RB-09).
/// Categorías deben existir al guardar; preguntas activas solo al activar la misión.
/// </summary>
internal static class MisionTriviaValidacion
{
    public static async Task AsegurarCategoriasExistenAsync(
        IReadOnlyList<CategoriaId> categoriaIds,
        ICategoriaRepository categoriaRepository,
        CancellationToken ct)
    {
        foreach (var categoriaId in categoriaIds)
        {
            var categoria = await categoriaRepository.FindByIdAsync(categoriaId, ct)
                ?? throw new NotFoundException(nameof(Categoria), categoriaId.Valor);

            if (categoria.Eliminada)
                throw new DomainException(
                    $"La categoría '{categoria.Nombre}' está eliminada y no puede usarse en una etapa Trivia.");
        }
    }

    public static async Task AsegurarPreguntasActivasParaActivacionAsync(
        Mision mision,
        IPreguntaRepository preguntaRepository,
        ICategoriaRepository categoriaRepository,
        CancellationToken ct)
    {
        foreach (var etapa in mision.Etapas.OfType<EtapaTrivia>())
        {
            await AsegurarCategoriasExistenAsync(etapa.CategoriaIds, categoriaRepository, ct);

            var tienePreguntasActivas = false;
            foreach (var categoriaId in etapa.CategoriaIds)
            {
                var preguntas = await preguntaRepository.FindByCategoriaAsync(categoriaId, ct);
                if (preguntas.Any(p => !p.Eliminada))
                {
                    tienePreguntasActivas = true;
                    break;
                }
            }

            if (!tienePreguntasActivas)
                throw new DomainException(
                    $"No se puede activar la misión: la etapa Trivia en orden {etapa.Orden} " +
                    "no tiene preguntas activas en sus categorías (RB-09).");
        }
    }
}
