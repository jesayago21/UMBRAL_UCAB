using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.AbandonarSesion;

public sealed record AbandonarSesionCommand(Guid SesionId, Guid JugadorId)
    : IRequest<Result<Unit>>;
