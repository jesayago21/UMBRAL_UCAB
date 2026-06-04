using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.AbrirInscripcionSesion;

public sealed record AbrirInscripcionSesionCommand(Guid SesionId)
    : IRequest<Result<Unit>>;
