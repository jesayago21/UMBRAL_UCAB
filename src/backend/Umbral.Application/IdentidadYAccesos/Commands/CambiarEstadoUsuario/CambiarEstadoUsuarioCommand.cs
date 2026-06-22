using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.IdentidadYAccesos.Commands.CambiarEstadoUsuario;

public sealed record CambiarEstadoUsuarioCommand(
    Guid KeycloakUserId,
    string Accion) : IRequest<Result<bool>>;
