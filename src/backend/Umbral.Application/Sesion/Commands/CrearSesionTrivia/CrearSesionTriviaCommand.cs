using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.CrearSesionTrivia;

/// <summary>
/// Operador crea sesión de trivia eligiendo una o más categorías del banco.
/// Solo entran preguntas activas asignadas a esas categorías.
/// </summary>
public sealed record CrearSesionTriviaCommand(
    IReadOnlyList<Guid> CategoriaIds,
    Guid OperadorId) : IRequest<Result<CrearSesionTriviaResult>>;
