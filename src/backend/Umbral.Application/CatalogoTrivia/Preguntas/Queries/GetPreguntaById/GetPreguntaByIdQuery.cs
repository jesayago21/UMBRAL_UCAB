using MediatR;
using Umbral.Application.CatalogoTrivia.Models;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Queries.GetPreguntaById;

public sealed record GetPreguntaByIdQuery(Guid PreguntaId) : IRequest<PreguntaDto>;
