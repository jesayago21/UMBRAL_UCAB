using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.CerrarPreguntaTrivia;

public sealed record CerrarPreguntaTriviaCommand(Guid SesionId)
    : IRequest<Result<Unit>>;
