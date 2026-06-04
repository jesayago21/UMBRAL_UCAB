using MediatR;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.Common.Models;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Commands.ActualizarPregunta;

public sealed record ActualizarPreguntaCommand(
    Guid PreguntaId,
    string Enunciado,
    string Dificultad,
    Guid? CategoriaId,
    IReadOnlyList<OpcionRespuestaInput> Opciones) : IRequest<Result<Guid>>;
