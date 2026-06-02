using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.CrearSesionMision;

public sealed record CrearSesionMisionCommand(
    Guid MisionId,
    Guid OperadorId) : IRequest<Result<CrearSesionMisionResult>>;

public sealed record CrearSesionMisionResult(
    Guid SesionId,
    string CodigoAcceso,
    string MisionNombre);
