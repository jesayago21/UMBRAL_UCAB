using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Commands.EliminarPregunta;

public sealed record EliminarPreguntaCommand(Guid PreguntaId) : IRequest<Result<Guid>>;
