using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Domain.IdentidadYAccesos;

/// <summary>
/// Proyección de lectura de un usuario desde Keycloak (puerto driven).
/// No es agregado de dominio ni entidad persistida localmente.
/// </summary>
public sealed record UsuarioIdentidad(
    KeycloakUserId KeycloakUserId,
    string Email,
    string Username,
    string Nombre,
    string Apellido,
    EstadoUsuario Estado,
    IReadOnlyList<RolSistema> Roles);
