using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.IdentidadYAccesos.Commands.EliminarUsuario;

public sealed record EliminarUsuarioCommand(
    Guid KeycloakUserId,
    Guid SolicitanteKeycloakUserId) : IRequest<Result<bool>>;
