using System.Net;
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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
            _logger.LogWarning(
                "KeycloakAdmin.UseDevStub=true: usuario {Username} NO se creó en Keycloak (Id ficticio {Id}). " +
                "Configure UseDevStub=false para login real.",
                username, devId);
            return KeycloakUserId.From(devId);
        }

        var token  = await ObtenerAdminTokenAsync(ct);
        var userId = await CrearUsuarioKeycloakAsync(token, email, username, nombre, apellido, ct);
        await EstablecerPasswordAsync(token, userId, passwordTemporal, ct);
        await SincronizarRolesKeycloakAsync(token, userId, roles, ct);
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
                "Keycloak dev stub: roles para {UserId}: {Roles}",
                userId.Value,
                string.Join(", ", roles));
            return;
        }

        var token = await ObtenerAdminTokenAsync(ct);
        await SincronizarRolesKeycloakAsync(token, userId, roles, ct);
    }

    public async Task CambiarEstadoAsync(KeycloakUserId userId, bool habilitado, CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            _logger.LogInformation(
                "Keycloak dev stub: estado {Estado} para {UserId}",
                habilitado ? "habilitado" : "bloqueado",
                userId.Value);
            return;
        }

        var token = await ObtenerAdminTokenAsync(ct);
        var url   = UserUrl(userId);

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { enabled = habilitado });

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task ActualizarPerfilAsync(
        KeycloakUserId userId,
        string nombre,
        string apellido,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            _logger.LogInformation("Keycloak dev stub: actualizar perfil {UserId}", userId.Value);
            return;
        }

        var token = await ObtenerAdminTokenAsync(ct);
        using var request = new HttpRequestMessage(HttpMethod.Put, UserUrl(userId));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            firstName = nombre,
            lastName  = apellido
        });

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task RestablecerPasswordAsync(
        KeycloakUserId userId,
        string nuevaPassword,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            _logger.LogInformation("Keycloak dev stub: restablecer password {UserId}", userId.Value);
            return;
        }

        var token = await ObtenerAdminTokenAsync(ct);
        await EstablecerPasswordAsync(token, userId, nuevaPassword, ct);
    }

    public async Task<KeycloakUserId?> ObtenerIdPorUsernameAsync(
        string username,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
            return null;

        var token = await ObtenerAdminTokenAsync(ct);
        var url =
            $"{_options.BaseUrl.TrimEnd('/')}/admin/realms/{_options.Realm}/users" +
            $"?username={Uri.EscapeDataString(username.Trim())}&exact=true";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserSummary>>(JsonOptions, ct);
        var match = users?.FirstOrDefault(u =>
            string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));

        return match is null ? null : KeycloakUserId.From(match.Id);
    }

    public async Task EliminarEnIdentityServerAsync(KeycloakUserId userId, CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            _logger.LogWarning("Keycloak dev stub: eliminar usuario {UserId}", userId.Value);
            return;
        }

        var token = await ObtenerAdminTokenAsync(ct);
        using var request = new HttpRequestMessage(HttpMethod.Delete, UserUrl(userId));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return;

        response.EnsureSuccessStatusCode();
    }

    private string UserUrl(KeycloakUserId userId) =>
        $"{_options.BaseUrl.TrimEnd('/')}/admin/realms/{_options.Realm}/users/{userId.Value:D}";

    private async Task<string> ObtenerAdminTokenAsync(CancellationToken ct)
    {
        var tokenUrl =
            $"{_options.BaseUrl.TrimEnd('/')}/realms/{_options.AdminTokenRealm}/protocol/openid-connect/token";

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"]  = _options.AdminClientId,
            ["username"]   = _options.AdminUsername,
            ["password"]   = _options.AdminPassword
        });

        var response = await _http.PostAsync(tokenUrl, content, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"No se pudo autenticar contra Keycloak Admin ({response.StatusCode}). " +
                $"Revise KeycloakAdmin (UseDevStub, AdminUsername/Password). Detalle: {body}");
        }

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
            requiredActions = Array.Empty<string>()
        });

        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Keycloak rechazó la creación del usuario ({response.StatusCode}): {body}");
        }

        if (response.Headers.Location is null)
            throw new InvalidOperationException("Keycloak no devolvió Location del usuario creado.");

        var idSegment = response.Headers.Location.Segments.Last().TrimEnd('/');
        return KeycloakUserId.From(Guid.Parse(idSegment));
    }

    private async Task EstablecerPasswordAsync(
        string token,
        KeycloakUserId userId,
        string password,
        CancellationToken ct)
    {
        var url = $"{UserUrl(userId)}/reset-password";

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            type      = "password",
            value     = password,
            temporary = false
        });

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task SincronizarRolesKeycloakAsync(
        string token,
        KeycloakUserId userId,
        IReadOnlyList<RolSistema> roles,
        CancellationToken ct)
    {
        var actuales = await ObtenerRolesRealmUsuarioAsync(token, userId, ct);
        if (actuales.Count > 0)
        {
            using var delete = new HttpRequestMessage(
                HttpMethod.Delete,
                $"{UserUrl(userId)}/role-mappings/realm");
            delete.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            delete.Content = JsonContent.Create(actuales);
            var deleteResponse = await _http.SendAsync(delete, ct);
            deleteResponse.EnsureSuccessStatusCode();
        }

        var destino = new List<KeycloakRoleRepresentation>();
        foreach (var rol in roles.Distinct())
        {
            var rep = await ObtenerRolRealmAsync(token, rol.ToString(), ct);
            if (rep is not null)
                destino.Add(rep);
        }

        if (destino.Count == 0)
            return;

        using var post = new HttpRequestMessage(
            HttpMethod.Post,
            $"{UserUrl(userId)}/role-mappings/realm");
        post.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        post.Content = JsonContent.Create(destino);

        var postResponse = await _http.SendAsync(post, ct);
        postResponse.EnsureSuccessStatusCode();
    }

    private async Task<KeycloakRoleRepresentation?> ObtenerRolRealmAsync(
        string token,
        string roleName,
        CancellationToken ct)
    {
        var url = $"{_options.BaseUrl.TrimEnd('/')}/admin/realms/{_options.Realm}/roles/{Uri.EscapeDataString(roleName)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<KeycloakRoleRepresentation>(JsonOptions, ct);
    }

    private async Task<List<KeycloakRoleRepresentation>> ObtenerRolesRealmUsuarioAsync(
        string token,
        KeycloakUserId userId,
        CancellationToken ct)
    {
        var url = $"{UserUrl(userId)}/role-mappings/realm";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return [];

        response.EnsureSuccessStatusCode();
        var roles = await response.Content.ReadFromJsonAsync<List<KeycloakRoleRepresentation>>(JsonOptions, ct);
        return roles ?? [];
    }

    private sealed class KeycloakRoleRepresentation
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class KeycloakUserSummary
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
    }
}
