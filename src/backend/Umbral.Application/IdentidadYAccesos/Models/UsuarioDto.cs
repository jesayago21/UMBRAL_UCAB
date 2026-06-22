namespace Umbral.Application.IdentidadYAccesos.Models;

/// <summary>
/// DTO de lectura (GET /usuarios, GET /usuarios/{id}).
/// El identificador es el UUID de Keycloak (única fuente de verdad).
/// </summary>
public sealed record UsuarioDto(
    Guid KeycloakUserId,
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    string Estado,
    IReadOnlyList<string> Roles);

public sealed record CrearUsuarioResult(
    Guid KeycloakUserId,
    string Email,
    string Estado,
    IReadOnlyList<string> Roles);
