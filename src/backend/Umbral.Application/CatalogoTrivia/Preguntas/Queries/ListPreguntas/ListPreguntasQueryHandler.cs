using MediatR;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Queries.ListPreguntas;

internal sealed class ListPreguntasQueryHandler
    : IRequestHandler<ListPreguntasQuery, IReadOnlyList<PreguntaDto>>
{
    private readonly IPreguntaRepository _preguntaRepository;

    public ListPreguntasQueryHandler(IPreguntaRepository preguntaRepository)
    {
        _preguntaRepository = preguntaRepository;
    }

    public async Task<IReadOnlyList<PreguntaDto>> Handle(
        ListPreguntasQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Pregunta> preguntas;

        if (query.CategoriaId.HasValue)
        {
            preguntas = await _preguntaRepository.FindByCategoriaAsync(
                new CategoriaId(query.CategoriaId.Value),
                cancellationToken);
        }
        else
        {
            preguntas = await _preguntaRepository.FindAllAsync(cancellationToken);
        }

        var activas = preguntas.Where(p => !p.Eliminada);

        if (!string.IsNullOrWhiteSpace(query.Dificultad))
        {
            var dificultad = TriviaMappings.ParseDificultad(query.Dificultad);
            activas = activas.Where(p => p.Dificultad == dificultad);
        }

        if (!string.IsNullOrWhiteSpace(query.Enunciado))
        {
            var filtro = query.Enunciado.Trim();
            activas = activas.Where(p =>
                p.Enunciado.Contains(filtro, StringComparison.OrdinalIgnoreCase));
        }

        return activas
            .OrderBy(p => p.Enunciado, StringComparer.OrdinalIgnoreCase)
            .Select(p => p.ToDto())
            .ToList();
    }
}
