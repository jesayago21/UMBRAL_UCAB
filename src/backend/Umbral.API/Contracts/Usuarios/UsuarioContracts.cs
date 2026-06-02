namespace Umbral.API.Contracts.Usuarios;

public sealed record CrearUsuarioRequest(
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string PasswordTemporal,
    IReadOnlyList<string> Roles);

public sealed record UsuarioResponse(
    Guid Id,
    Guid KeycloakUserId,
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string Estado,
    IReadOnlyList<string> Roles,
    string? PasswordAsignada);

public sealed record AsignarRolesRequest(IReadOnlyList<string> Roles);

public sealed record CambiarEstadoUsuarioRequest(string Accion);

public sealed record ActualizarUsuarioRequest(
    string Nombre,
    string Apellido,
    string Rol,
    string? NuevaPassword);
