using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Umbral.API.Auth;
using Umbral.API.Contracts.Trivia;
using Umbral.API.Tests.Support;

namespace Umbral.API.Tests.Controllers;

[Collection(nameof(ApiCollection))]
public sealed class PreguntasControllerTests
{
    private readonly HttpClient _client;

    public PreguntasControllerTests(ApiIntegrationFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task POST_preguntas_CuandoAdmin_Retorna201()
    {
        SetRole("Administrador");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/preguntas",
            BuildCrearRequest("Enunciado POST API de prueba"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_preguntas_CuandoOperador_Retorna403()
    {
        SetRole("Operador");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/preguntas",
            BuildCrearRequest("Enunciado operador API de prueba"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GET_preguntas_CuandoAdmin_Retorna200()
    {
        SetRole("Administrador");
        var enunciado = $"Enunciado listado API {Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/v1/preguntas", BuildCrearRequest(enunciado));

        var response = await _client.GetAsync("/api/v1/preguntas?dificultad=Facil");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<PreguntaResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(x => x.Enunciado == enunciado);
    }

    [Fact]
    public async Task PUT_preguntas_CuandoAdmin_ActualizaEnunciado()
    {
        SetRole("Administrador");
        var create = await _client.PostAsJsonAsync(
            "/api/v1/preguntas",
            BuildCrearRequest("Enunciado editar API de prueba"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadCreatedId(create);

        var update = await _client.PutAsJsonAsync(
            $"/api/v1/preguntas/{id}",
            new ActualizarPreguntaRequest(
                "Enunciado editado API de prueba",
                "Dificil",
                null,
                BuildOpcionesValidas()));

        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/v1/preguntas/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<PreguntaResponse>();
        body!.Enunciado.Should().Be("Enunciado editado API de prueba");
        body.Dificultad.Should().Be("Dificil");
    }

    [Fact]
    public async Task DELETE_preguntas_CuandoExiste_EliminaYRetorna204()
    {
        SetRole("Administrador");
        var create = await _client.PostAsJsonAsync(
            "/api/v1/preguntas",
            BuildCrearRequest($"Enunciado eliminar API {Guid.NewGuid():N}"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadCreatedId(create);

        var delete = await _client.DeleteAsync($"/api/v1/preguntas/{id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/v1/preguntas/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_preguntas_por_id_CuandoNoExiste_Retorna404()
    {
        SetRole("Administrador");
        var response = await _client.GetAsync($"/api/v1/preguntas/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_preguntas_ConCategoriaValida_Retorna201()
    {
        SetRole("Administrador");
        var categoria = await _client.PostAsJsonAsync(
            "/api/v1/categorias",
            new CrearCategoriaRequest($"Categoría pregunta API {Guid.NewGuid():N}"));
        var categoriaId = await ReadCreatedId(categoria);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/preguntas",
            BuildCrearRequest("Enunciado con categoría API de prueba", categoriaId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var id = await ReadCreatedId(response);
        var get = await _client.GetAsync($"/api/v1/preguntas/{id}");
        var body = await get.Content.ReadFromJsonAsync<PreguntaResponse>();
        body!.CategoriaId.Should().Be(categoriaId);
    }

    private void SetRole(string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeaderName);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
    }

    private static CrearPreguntaRequest BuildCrearRequest(
        string enunciado,
        Guid? categoriaId = null) =>
        new(
            enunciado,
            "Facil",
            categoriaId,
            BuildOpcionesValidas());

    private static List<OpcionRespuestaRequest> BuildOpcionesValidas() =>
    [
        new("Opción correcta API de prueba", true),
        new("Opción incorrecta 1 API de prueba", false),
        new("Opción incorrecta 2 API de prueba", false)
    ];

    private static async Task<Guid> ReadCreatedId(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        return body!["id"];
    }
}
