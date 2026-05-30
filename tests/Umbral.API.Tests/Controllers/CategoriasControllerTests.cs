using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Umbral.API.Auth;
using Umbral.API.Contracts.Trivia;
using Umbral.API.Tests.Support;

namespace Umbral.API.Tests.Controllers;

[Collection(nameof(ApiCollection))]
public sealed class CategoriasControllerTests
{
    private readonly HttpClient _client;

    public CategoriasControllerTests(ApiIntegrationFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task POST_categorias_CuandoAdmin_Retorna201()
    {
        SetRole("Administrador");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/categorias",
            new CrearCategoriaRequest($"Categoría API admin {Guid.NewGuid():N}"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_categorias_CuandoOperador_Retorna403()
    {
        SetRole("Operador");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/categorias",
            new CrearCategoriaRequest("Categoría API operador de prueba"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GET_categorias_CuandoAdmin_Retorna200()
    {
        SetRole("Administrador");
        var nombre = $"Categoría listado API {Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/v1/categorias", new CrearCategoriaRequest(nombre));

        var response = await _client.GetAsync($"/api/v1/categorias?nombre={Uri.EscapeDataString("listado API")}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<CategoriaResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(x => x.Nombre == nombre);
    }

    [Fact]
    public async Task PUT_categorias_CuandoAdmin_ActualizaNombre()
    {
        SetRole("Administrador");
        var create = await _client.PostAsJsonAsync(
            "/api/v1/categorias",
            new CrearCategoriaRequest("Categoría editar API de prueba"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadCreatedId(create);

        var update = await _client.PutAsJsonAsync(
            $"/api/v1/categorias/{id}",
            new ActualizarCategoriaRequest("Categoría editada API de prueba"));

        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/v1/categorias/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<CategoriaResponse>();
        body!.Nombre.Should().Be("Categoría editada API de prueba");
    }

    [Fact]
    public async Task DELETE_categorias_CuandoExiste_EliminaYRetorna204()
    {
        SetRole("Administrador");
        var create = await _client.PostAsJsonAsync(
            "/api/v1/categorias",
            new CrearCategoriaRequest($"Categoría eliminar API {Guid.NewGuid():N}"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadCreatedId(create);

        var delete = await _client.DeleteAsync($"/api/v1/categorias/{id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/v1/categorias/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_categorias_por_id_CuandoNoExiste_Retorna404()
    {
        SetRole("Administrador");
        var response = await _client.GetAsync($"/api/v1/categorias/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_categorias_CuandoNombreDuplicado_Retorna400()
    {
        SetRole("Administrador");
        var nombre = $"Categoría duplicada API {Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/v1/categorias", new CrearCategoriaRequest(nombre));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/categorias",
            new CrearCategoriaRequest(nombre));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    private void SetRole(string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeaderName);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
    }

    private static async Task<Guid> ReadCreatedId(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        return body!["id"];
    }
}
