using MediatR;
using Umbral.Application.CatalogoTrivia.Models;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Queries.ListPreguntas;

public sealed record ListPreguntasQuery(
    Guid? CategoriaId = null,
    string? Dificultad = null,
    string? Enunciado = null) : IRequest<IReadOnlyList<PreguntaDto>>;
