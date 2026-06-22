using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Umbral.API.Auth;
using Umbral.API.Contracts.Misiones;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Tests.Support;

namespace Umbral.API.Tests.Controllers;

[Collection(nameof(ApiCollection))]
public sealed class MisionesControllerTests
{
    private readonly HttpClient _client;

    public MisionesControllerTests(ApiIntegrationFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task POST_misiones_CuandoAdmin_Retorna201()
    {
        SetRole("Administrador");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/misiones",
            BuildCrearRequest("Mision API Admin"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_misiones_CuandoOperador_Retorna403()
    {
        SetRole("Operador");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/misiones",
            BuildCrearRequest("Mision API Operador"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GET_misiones_CuandoAdmin_Retorna200()
    {
        SetRole("Administrador");
        await _client.PostAsJsonAsync("/api/v1/misiones", BuildCrearRequest("Mision Listado"));

        var response = await _client.GetAsync("/api/v1/misiones");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<MisionResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(x => x.Nombre == "Mision Listado");
    }

    [Fact]
    public async Task PUT_misiones_CuandoAdmin_ActualizaNombreYEstado()
    {
        SetRole("Administrador");
        var create = await _client.PostAsJsonAsync("/api/v1/misiones", BuildCrearRequest("Mision Editar", activar: false));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadCreatedId(create);

        var update = await _client.PutAsJsonAsync(
            $"/api/v1/misiones/{id}",
            new ActualizarMisionRequest("Mision Editada", true));

        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/v1/misiones/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await get.Content.ReadFromJsonAsync<MisionResponse>();
        body!.Nombre.Should().Be("Mision Editada");
        body.Estado.Should().Be("Activa");
    }

    [Fact]
    public async Task DELETE_misiones_CuandoSinSesiones_EliminaMisionYEtapas_Retorna204()
    {
        SetRole("Administrador");
        var create = await _client.PostAsJsonAsync("/api/v1/misiones", BuildCrearRequest("Mision Eliminar", activar: true));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadCreatedId(create);

        var delete = await _client.DeleteAsync($"/api/v1/misiones/{id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/v1/misiones/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_misiones_por_id_CuandoNoExiste_Retorna404()
    {
        SetRole("Administrador");
        var response = await _client.GetAsync($"/api/v1/misiones/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_misiones_CuandoNombreDuplicado_Retorna400()
    {
        SetRole("Administrador");
        await _client.PostAsJsonAsync("/api/v1/misiones", BuildCrearRequest("Mision Duplicada"));

        var response = await _client.PostAsJsonAsync(
            "/api/v1/misiones",
            BuildCrearRequest("Mision Duplicada"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task PUT_misiones_CuandoTieneSesionActiva_Retorna400()
    {
        SetRole("Administrador");
        var create = await _client.PostAsJsonAsync("/api/v1/misiones", BuildCrearRequest("Mision En Uso", activar: true));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadCreatedId(create);

        var crearSesion = await _client.PostAsJsonAsync(
            "/api/v1/sesiones",
            new CrearSesionMisionRequest(id, "Sesión misión en uso"));
        crearSesion.EnsureSuccessStatusCode();
        var sesion = (await crearSesion.Content.ReadFromJsonAsync<CrearSesionMisionResponse>())!;

        SetRole("Participante");
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeaderName);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, Guid.NewGuid().ToString());
        await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesion.Id}/unirse",
            new UnirseSesionRequest(sesion.CodigoAcceso, "Alpha"));
        SetRole("Administrador");
        await _client.PostAsync($"/api/v1/sesiones/{sesion.Id}/iniciar", null);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/misiones/{id}",
            new ActualizarMisionRequest("Mision Renombrada", true));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task GET_misiones_activas_CuandoOperador_Retorna200()
    {
        SetRole("Administrador");
        var nombre = $"Mision activa operador {Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/v1/misiones", BuildCrearRequest(nombre, activar: true));

        SetRole("Operador");
        var response = await _client.GetAsync("/api/v1/misiones/activas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<MisionActivaResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(x => x.Nombre == nombre);
    }

    [Fact]
    public async Task GET_misiones_activas_CuandoParticipante_Retorna403()
    {
        SetRole("Participante");
        var response = await _client.GetAsync("/api/v1/misiones/activas");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private void SetRole(string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeaderName);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
    }

    private static CrearMisionRequest BuildCrearRequest(string nombre, bool activar = true) =>
        new(
            nombre,
            new List<CrearEtapaRequest>
            {
                new(
                    "BusquedaTesoro",
                    1,
                    "Etapa 1",
                    "QR-MISION-001",
                    new List<CrearPistaRequest>
                    {
                        new("Pista 1", "PorTiempo", 30)
                    },
                    null)
            },
            activar);

    private static async Task<Guid> ReadCreatedId(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        return body!["id"];
    }
}
