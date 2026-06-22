using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Umbral.API.Auth;
using Umbral.API.Contracts.Usuarios;
using Umbral.API.Tests.Support;

namespace Umbral.API.Tests.Controllers;

[Collection(nameof(ApiCollection))]
public sealed class UsuariosControllerTests
{
    private readonly HttpClient _client;

    public UsuariosControllerTests(ApiIntegrationFixture fixture)
        => _client = fixture.Factory.CreateClient();

    [Fact]
    public async Task POST_usuarios_CuandoAdmin_Retorna201()
    {
        SetRole("Administrador");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/usuarios",
            BuildCrearRequest("admin_crear@test.com", "admin_crear"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<CrearUsuarioResponse>();
        body!.Email.Should().Be("admin_crear@test.com");
        body.Roles.Should().Contain("Operador");
        body.PasswordTemporal.Should().NotBeNullOrWhiteSpace();
        body.KeycloakUserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task POST_usuarios_CuandoOperador_Retorna403()
    {
        SetRole("Operador");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/usuarios",
            BuildCrearRequest("operador@test.com", "operador_user"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task POST_usuarios_CuandoEmailDuplicado_Retorna400()
    {
        SetRole("Administrador");
        var request = BuildCrearRequest("dup@test.com", "dup_user");
        await _client.PostAsJsonAsync("/api/v1/usuarios", request);

        var response = await _client.PostAsJsonAsync("/api/v1/usuarios", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_usuarios_CuandoRolesParticipante_Retorna400_RB35()
    {
        SetRole("Administrador");

        var email = $"part_{Guid.NewGuid():N}@test.com";
        var response = await _client.PostAsJsonAsync(
            "/api/v1/usuarios",
            new CrearUsuarioRequest(
                email,
                $"part_user_{Guid.NewGuid():N}",
                "Part",
                "Test",
                "Password1!",
                ["Participante"]));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_usuarios_CuandoAdmin_Retorna200()
    {
        SetRole("Administrador");
        await _client.PostAsJsonAsync(
            "/api/v1/usuarios",
            BuildCrearRequest("listado@test.com", "listado_user"));

        var response = await _client.GetAsync("/api/v1/usuarios");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<UsuarioResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(u => u.Email == "listado@test.com");
    }

    [Fact]
    public async Task GET_usuarios_id_CuandoNoExiste_Retorna404()
    {
        SetRole("Administrador");

        var response = await _client.GetAsync($"/api/v1/usuarios/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PUT_usuarios_roles_CuandoAdmin_Retorna204()
    {
        SetRole("Administrador");
        var create = await _client.PostAsJsonAsync(
            "/api/v1/usuarios",
            BuildCrearRequest("roles@test.com", "roles_user"));
        create.EnsureSuccessStatusCode();
        var usuario = (await create.Content.ReadFromJsonAsync<CrearUsuarioResponse>())!;

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/usuarios/{usuario.KeycloakUserId}/roles",
            new AsignarRolesRequest(["Administrador"]));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PUT_usuarios_estado_Bloquear_Retorna204()
    {
        SetRole("Administrador");
        var create = await _client.PostAsJsonAsync(
            "/api/v1/usuarios",
            BuildCrearRequest("estado@test.com", "estado_user"));
        create.EnsureSuccessStatusCode();
        var usuario = (await create.Content.ReadFromJsonAsync<CrearUsuarioResponse>())!;

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/usuarios/{usuario.KeycloakUserId}/estado",
            new CambiarEstadoUsuarioRequest("Bloquear"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GET_autenticacion_me_CuandoAutenticado_Retorna200()
    {
        SetRole("Administrador");

        var response = await _client.GetAsync("/api/v1/autenticacion/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<MeResponse>();
        body!.Roles.Should().Contain("Administrador");
    }

    private void SetRole(string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeaderName);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
    }

    private static CrearUsuarioRequest BuildCrearRequest(string email, string username) =>
        new(email, username, "Nombre", "Apellido", "Password1", ["Operador"]);
}
