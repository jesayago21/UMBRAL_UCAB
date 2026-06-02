using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.IdentidadYAccesos.Commands.AsignarRolesUsuario;

public sealed record AsignarRolesUsuarioCommand(
    Guid UsuarioId,
    IReadOnlyList<string> Roles) : IRequest<Result<bool>>;
