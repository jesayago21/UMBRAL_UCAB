using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.UnirseSesion;

public sealed record UnirseSesionCommand(
    Guid SesionId,
    string CodigoAcceso,
    Guid JugadorId,
    string NombreEquipo) : IRequest<Result<UnirseSesionResult>>;

public sealed record UnirseSesionResult(Guid EquipoId);
