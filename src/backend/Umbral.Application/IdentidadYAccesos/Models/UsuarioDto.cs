namespace Umbral.Application.IdentidadYAccesos.Models;

/// <summary>
/// DTO de lectura (GET /usuarios, GET /usuarios/{id}).
/// No incluye contraseña: las credenciales se devuelven únicamente en la respuesta 201 de creación.
/// </summary>
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
