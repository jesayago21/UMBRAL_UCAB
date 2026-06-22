namespace Umbral.API.Contracts.Usuarios;

public sealed record CrearUsuarioRequest(
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string PasswordTemporal,
    IReadOnlyList<string> Roles);

/// <summary>
/// Respuesta exclusiva del POST 201: incluye la contraseña temporal para que el admin
/// pueda entregarla al usuario en el momento de la creación.
/// </summary>
public sealed record CrearUsuarioResponse(
    Guid KeycloakUserId,
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string Estado,
    IReadOnlyList<string> Roles,
    string PasswordTemporal);

/// <summary>
/// Respuesta de lectura (GET /usuarios, GET /usuarios/{id}).
/// </summary>
public sealed record UsuarioResponse(
    Guid KeycloakUserId,
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string Estado,
    IReadOnlyList<string> Roles);

public sealed record AsignarRolesRequest(IReadOnlyList<string> Roles);

public sealed record CambiarEstadoUsuarioRequest(string Accion);

public sealed record ActualizarUsuarioRequest(
    string Nombre,
    string Apellido,
    string Rol,
    string? NuevaPassword);

public sealed record MeResponse(
    string Sub,
    string Username,
    string Email,
    IReadOnlyList<string> Roles);
