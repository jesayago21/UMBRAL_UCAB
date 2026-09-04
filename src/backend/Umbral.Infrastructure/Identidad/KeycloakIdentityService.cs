using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbral.Domain.IdentidadYAccesos;
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
        IReadOnlyList<RolSistema> roles,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            var devId = KeycloakDevStubStore.Registrar(email, username, nombre, apellido, roles);
            _logger.LogWarning(
                "KeycloakAdmin.UseDevStub=true: usuario {Username} registrado en memoria (Id {Id}).",
                username, devId.Value);
            return await Task.FromResult(devId);
        }

        var token  = await ObtenerAdminTokenAsync(ct);
        var userId = await CrearUsuarioKeycloakAsync(
            token, email, username, nombre, apellido, requireUpdatePassword: true, ct);
        try
        {
            await SincronizarRolesKeycloakAsync(token, userId, roles, ct);
            await EnviarAccionesRequeridasEmailAsync(token, userId, ct);
        }
        catch
        {
            try
            {
                await EliminarEnIdentityServerAsync(userId, ct);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning(
                    cleanupEx,
                    "No se pudo revertir el usuario Keycloak {UserId} tras fallo de post-registro.",
                    userId.Value);
            }

            throw;
        }

        return userId;
    }

    public async Task<KeycloakUserId> RegistrarParticipanteEnIdentityServerAsync(
        EmailAddress email,
        string username,
        string nombre,
        string apellido,
        string password,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.", nameof(password));

        var roles = new[] { RolSistema.Participante };

        if (_options.UseDevStub)
        {
            var devId = KeycloakDevStubStore.Registrar(email, username, nombre, apellido, roles);
            _logger.LogWarning(
                "KeycloakAdmin.UseDevStub=true: participante {Username} registrado en memoria (Id {Id}).",
                username, devId.Value);
            return await Task.FromResult(devId);
        }

        var token  = await ObtenerAdminTokenAsync(ct);
        var userId = await CrearUsuarioKeycloakAsync(
            token, email, username, nombre, apellido, requireUpdatePassword: false, ct);
        try
        {
            await EstablecerPasswordAsync(token, userId, password, ct);
            await SincronizarRolesKeycloakAsync(token, userId, roles, ct);
        }
        catch
        {
            try
            {
                await EliminarEnIdentityServerAsync(userId, ct);
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning(
                    cleanupEx,
                    "No se pudo revertir el participante Keycloak {UserId} tras fallo de post-registro.",
                    userId.Value);
            }

            throw;
        }

        return userId;
    }

    public async Task SincronizarRolesAsync(
        KeycloakUserId userId,
        IReadOnlyList<RolSistema> roles,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            KeycloakDevStubStore.SincronizarRoles(userId, roles);
            return;
        }

        var token = await ObtenerAdminTokenAsync(ct);
        await SincronizarRolesKeycloakAsync(token, userId, roles, ct);
    }

    public async Task CambiarEstadoAsync(KeycloakUserId userId, bool habilitado, CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            KeycloakDevStubStore.CambiarEstado(userId, habilitado);
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
            KeycloakDevStubStore.ActualizarPerfil(userId, nombre, apellido);
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
            return;

        var token = await ObtenerAdminTokenAsync(ct);
        await EstablecerPasswordAsync(token, userId, nuevaPassword, ct);
    }

    public async Task<KeycloakUserId?> ObtenerIdPorUsernameAsync(
        string username,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
            return await Task.FromResult(KeycloakDevStubStore.ObtenerIdPorUsername(username));

        var token = await ObtenerAdminTokenAsync(ct);
        var users = await BuscarUsuariosAsync(token, username: username.Trim(), exact: true, ct: ct);
        var match = users.FirstOrDefault(u =>
            string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));

        return match is null ? null : KeycloakUserId.From(match.Id);
    }

    public async Task<bool> ExisteEmailAsync(EmailAddress email, CancellationToken ct = default)
    {
        if (_options.UseDevStub)
            return await Task.FromResult(KeycloakDevStubStore.ExisteEmail(email));

        var token = await ObtenerAdminTokenAsync(ct);
        var users = await BuscarUsuariosAsync(token, email: email.Value, exact: true, ct: ct);
        return users.Any(u =>
            string.Equals(u.Email, email.Value, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExisteUsernameAsync(string username, CancellationToken ct = default)
    {
        if (_options.UseDevStub)
            return await Task.FromResult(KeycloakDevStubStore.ExisteUsername(username));

        var token = await ObtenerAdminTokenAsync(ct);
        var users = await BuscarUsuariosAsync(token, username: username.Trim(), exact: true, ct: ct);
        return users.Any(u =>
            string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public async Task<UsuarioIdentidad?> ObtenerUsuarioPorIdAsync(
        KeycloakUserId userId,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
            return await Task.FromResult(KeycloakDevStubStore.ObtenerPorId(userId));

        var token = await ObtenerAdminTokenAsync(ct);
        using var request = new HttpRequestMessage(HttpMethod.Get, UserUrl(userId));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<KeycloakUserRepresentation>(JsonOptions, ct);
        if (user is null)
            return null;

        var roles = await ObtenerRolesKeycloakAsync(token, userId, ct);
        return MapToUsuarioIdentidad(user, roles);
    }

    public async Task<IReadOnlyList<UsuarioIdentidad>> ListarUsuariosAsync(
        int first,
        int max,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
            return await Task.FromResult(KeycloakDevStubStore.Listar(first, max));

        var token = await ObtenerAdminTokenAsync(ct);
        var url =
            $"{UsersCollectionUrl()}?first={Math.Max(0, first)}&max={Math.Clamp(max, 1, 100)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserRepresentation>>(JsonOptions, ct)
                    ?? [];

        var result = new List<UsuarioIdentidad>(users.Count);
        foreach (var user in users)
        {
            var userId = KeycloakUserId.From(user.Id);
            var roles  = await ObtenerRolesKeycloakAsync(token, userId, ct);
            result.Add(MapToUsuarioIdentidad(user, roles));
        }

        return result;
    }

    public async Task<IReadOnlyList<RolSistema>> ObtenerRolesAsync(
        KeycloakUserId userId,
        CancellationToken ct = default)
    {
        if (_options.UseDevStub)
            return await Task.FromResult(KeycloakDevStubStore.ObtenerRoles(userId));

        var token = await ObtenerAdminTokenAsync(ct);
        return await ObtenerRolesKeycloakAsync(token, userId, ct);
    }

    public async Task EliminarEnIdentityServerAsync(KeycloakUserId userId, CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            KeycloakDevStubStore.Eliminar(userId);
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

    private string UsersCollectionUrl() =>
        $"{_options.BaseUrl.TrimEnd('/')}/admin/realms/{_options.Realm}/users";

    private string UserUrl(KeycloakUserId userId) =>
        $"{UsersCollectionUrl()}/{userId.Value:D}";

    private async Task<List<KeycloakUserRepresentation>> BuscarUsuariosAsync(
        string token,
        string? username = null,
        string? email = null,
        bool exact = false,
        CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(username))
            query.Add($"username={Uri.EscapeDataString(username)}");
        if (!string.IsNullOrWhiteSpace(email))
            query.Add($"email={Uri.EscapeDataString(email)}");
        if (exact)
            query.Add("exact=true");

        var url = query.Count == 0
            ? UsersCollectionUrl()
            : $"{UsersCollectionUrl()}?{string.Join("&", query)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<KeycloakUserRepresentation>>(JsonOptions, ct)
               ?? [];
    }

    private static UsuarioIdentidad MapToUsuarioIdentidad(
        KeycloakUserRepresentation user,
        IReadOnlyList<RolSistema> roles) =>
        new(
            KeycloakUserId.From(user.Id),
            user.Email ?? string.Empty,
            user.Username ?? string.Empty,
            user.FirstName ?? string.Empty,
            user.LastName ?? string.Empty,
            user.Enabled ? EstadoUsuario.Activo : EstadoUsuario.Bloqueado,
            roles);

    private async Task<IReadOnlyList<RolSistema>> ObtenerRolesKeycloakAsync(
        string token,
        KeycloakUserId userId,
        CancellationToken ct)
    {
        var reps = await ObtenerRolesRealmUsuarioAsync(token, userId, ct);
        return reps
            .Select(r => r.Name)
            .Where(n => Enum.TryParse<RolSistema>(n, ignoreCase: true, out _))
            .Select(n => Enum.Parse<RolSistema>(n, ignoreCase: true))
            .Distinct()
            .ToList();
    }

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
        bool requireUpdatePassword,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, UsersCollectionUrl());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        object payload = requireUpdatePassword
            ? new
            {
                email         = email.Value,
                username,
                firstName     = nombre,
                lastName      = apellido,
                enabled       = true,
                emailVerified = true,
                requiredActions = new[] { "UPDATE_PASSWORD" }
            }
            : new
            {
                email         = email.Value,
                username,
                firstName     = nombre,
                lastName      = apellido,
                enabled       = true,
                emailVerified = true
            };

        request.Content = JsonContent.Create(payload);

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

    private async Task EnviarAccionesRequeridasEmailAsync(
        string token,
        KeycloakUserId userId,
        CancellationToken ct)
    {
        var query =
            $"client_id={Uri.EscapeDataString(_options.FrontendClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(_options.FrontendRedirectUri)}";
        var url = $"{UserUrl(userId)}/execute-actions-email?{query}";

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new[] { "UPDATE_PASSWORD" });

        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Keycloak no pudo enviar el email de configuración de contraseña ({response.StatusCode}). " +
                "Verifique SMTP del realm (Mailpit en local). Detalle: " + body);
        }

        _logger.LogInformation(
            "Email execute-actions (UPDATE_PASSWORD) enviado para usuario Keycloak {UserId}.",
            userId.Value);
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

    private sealed class KeycloakUserRepresentation
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool Enabled { get; set; }
    }
}
