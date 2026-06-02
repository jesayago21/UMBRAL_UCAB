namespace Umbral.Application.IdentidadYAccesos.Models;

public sealed record UsuarioDto(
    Guid Id,
    Guid KeycloakUserId,
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string Estado,
    IReadOnlyList<string> Roles);

public sealed record CrearUsuarioResult(
    Guid UsuarioId,
    Guid KeycloakUserId,
    string Email,
    string Estado);
