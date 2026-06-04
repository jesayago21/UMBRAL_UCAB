namespace Umbral.Infrastructure.Identidad;

public sealed class KeycloakAdminOptions
{
    public const string SectionName = "KeycloakAdmin";

    public string BaseUrl { get; init; } = "http://localhost:8080";
    public string Realm { get; init; } = "umbral";
    /// <summary>Realm usado solo para obtener token admin (p. ej. master).</summary>
    public string AdminTokenRealm { get; init; } = "master";
    public string AdminClientId { get; init; } = "admin-cli";
    public string AdminUsername { get; init; } = "admin";
    public string AdminPassword { get; init; } = "admin";
    public bool UseDevStub { get; init; }
}
