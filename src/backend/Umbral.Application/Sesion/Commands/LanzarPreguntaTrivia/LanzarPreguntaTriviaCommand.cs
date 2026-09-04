using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.LanzarPreguntaTrivia;

public sealed record LanzarPreguntaTriviaCommand(
    Guid SesionId,
    int? DuracionSegundos = null) : IRequest<Result<Unit>>;
