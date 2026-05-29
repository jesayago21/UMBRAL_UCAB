namespace Umbral.API.Auth;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>
    /// URL del realm (issuer). Ej: http://localhost:8080/realms/umbral
    /// </summary>
    public string Authority { get; init; } = "http://localhost:8080/realms/umbral";

    /// <summary>
    /// Audiencia esperada (clientId del resource server). Vacío = no validar audiencia.
    /// </summary>
    public string Audience { get; init; } = "umbral-api";

    /// <summary>
    /// Permite HTTP (sin TLS) hacia Keycloak en desarrollo local.
    /// </summary>
    public bool RequireHttpsMetadata { get; init; }
}
