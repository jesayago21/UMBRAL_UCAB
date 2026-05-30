using MediatR;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.Common.Models;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Commands.CrearPregunta;

public sealed record CrearPreguntaCommand(
    string Enunciado,
    string Dificultad,
    Guid? CategoriaId,
    IReadOnlyList<OpcionRespuestaInput> Opciones) : IRequest<Result<Guid>>;
