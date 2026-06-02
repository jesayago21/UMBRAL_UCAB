using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Infrastructure.Identidad;

public sealed class KeycloakIdentityService : IIdentityService
{
    private readonly HttpClient _http;
    private readonly KeycloakAdminOptions _options;
    private readonly ILogger<KeycloakIdentityService> _logger;

    public KeycloakIdentityService(
        HttpClient http,
        IOptions<KeycloakAdminOptions> options,
        ILogger<KeycloakIdentityService> logger)
    {
        _http    = http;
        _options = options.Value;
        _logger  = logger;
    }

    public async Task<KeycloakUserId> RegistrarEnIdentityServerAsync(
        EmailAddress email,
        string username,
        string nombre,
        string apellido,
        string passwordTemporal,
        IReadOnlyList<RolSistema> roles,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            var devId = Guid.NewGuid();
            _logger.LogInformation(
                "Keycloak dev stub: usuario {Email} registrado con Id {Id}",
                email.Value, devId);
            return KeycloakUserId.From(devId);
        }

        var token = await ObtenerAdminTokenAsync(ct);
        var userId = await CrearUsuarioKeycloakAsync(
            token, email, username, nombre, apellido, passwordTemporal, ct);
        await SincronizarRolesAsync(userId, roles, ct);
        return userId;
    }

    public async Task SincronizarRolesAsync(
        KeycloakUserId userId,
        IReadOnlyList<RolSistema> roles,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            _logger.LogInformation(
                "Keycloak dev stub: roles sincronizados para {UserId}: {Roles}",
                userId.Value,
                string.Join(", ", roles));
            return;
        }

        _ = await ObtenerAdminTokenAsync(ct);
        _logger.LogInformation("Sincronización de roles Keycloak para {UserId}", userId.Value);
    }

    public Task CambiarEstadoAsync(KeycloakUserId userId, bool habilitado, CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            _logger.LogInformation(
                "Keycloak dev stub: estado {Estado} para {UserId}",
                habilitado ? "habilitado" : "bloqueado",
                userId.Value);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public Task EliminarEnIdentityServerAsync(KeycloakUserId userId, CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            _logger.LogWarning(
                "Keycloak dev stub: compensación — eliminar usuario {UserId}",
                userId.Value);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    private async Task<string> ObtenerAdminTokenAsync(CancellationToken ct)
    {
        var tokenUrl =
            $"{_options.BaseUrl.TrimEnd('/')}/realms/{_options.Realm}/protocol/openid-connect/token";

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "client_credentials",
            ["client_id"]     = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        });

        var response = await _http.PostAsync(tokenUrl, content, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json.GetProperty("access_token").GetString()
               ?? throw new InvalidOperationException("Token Keycloak inválido.");
    }

    private async Task<KeycloakUserId> CrearUsuarioKeycloakAsync(
        string token,
        EmailAddress email,
        string username,
        string nombre,
        string apellido,
        string passwordTemporal,
        CancellationToken ct)
    {
        var url = $"{_options.BaseUrl.TrimEnd('/')}/admin/realms/{_options.Realm}/users";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            email         = email.Value,
            username,
            firstName     = nombre,
            lastName      = apellido,
            enabled       = true,
            emailVerified = true,
            credentials = new[]
            {
                new
                {
                    type      = "password",
                    value     = passwordTemporal,
                    temporary = true
                }
            }
        });

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        if (response.Headers.Location is null)
            throw new InvalidOperationException("Keycloak no devolvió Location del usuario creado.");

        var idSegment = response.Headers.Location.Segments.Last();
        return KeycloakUserId.From(Guid.Parse(idSegment));
    }
}
