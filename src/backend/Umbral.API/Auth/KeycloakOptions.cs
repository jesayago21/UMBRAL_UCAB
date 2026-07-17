namespace Umbral.API.Auth;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>
    /// URL del realm (issuer). Ej: http://localhost:8080/realms/umbral
    /// Debe coincidir con el <c>iss</c> del token (lo que ve el navegador al loguearse).
    /// </summary>
    public string Authority { get; init; } = "http://localhost:8080/realms/umbral";

    /// <summary>
    /// Opcional. URL de metadata OIDC para obtener JWKS cuando el API no puede
    /// resolver <see cref="Authority"/> (p. ej. API en Docker, Keycloak en red interna).
    /// Ej: http://keycloak:8080/realms/umbral
    /// </summary>
    public string? MetadataAddress { get; init; }

    /// <summary>
    /// Audiencia esperada (clientId del resource server). Vacío = no validar audiencia.
    /// </summary>
    public string Audience { get; init; } = "umbral-api";

    /// <summary>
    /// Permite HTTP (sin TLS) hacia Keycloak en desarrollo local.
    /// </summary>
    public bool RequireHttpsMetadata { get; init; }
}
