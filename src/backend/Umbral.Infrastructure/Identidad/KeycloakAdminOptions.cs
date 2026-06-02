namespace Umbral.Infrastructure.Identidad;

public sealed class KeycloakAdminOptions
{
    public const string SectionName = "KeycloakAdmin";

    public string BaseUrl { get; init; } = "http://localhost:8080";
    public string Realm { get; init; } = "umbral";
    public string ClientId { get; init; } = "umbral-api";
    public string ClientSecret { get; init; } = string.Empty;
    public bool UseDevStub { get; init; } = true;
}
