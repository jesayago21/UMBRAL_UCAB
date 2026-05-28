using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Umbral.API.Contracts.Sesiones;
using Umbral.API.Tests.Support;

namespace Umbral.API.Tests.Controllers;

[Collection(nameof(ApiCollection))]
public sealed class SesionesControllerTests
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _services;

    public SesionesControllerTests(ApiIntegrationFixture fixture)
    {
        _client    = fixture.Factory.CreateClient();
        _services  = fixture.Factory.Services;
    }

    [Fact]
    public async Task POST_busqueda_tesoro_CuandoMisionActiva_Retorna201()
    {
        var misionId = await ApiTestData.SeedMisionActivaAsync(_services);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/busqueda-tesoro",
            new CrearSesionBusquedaTesoroRequest(misionId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<CrearSesionResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task POST_busqueda_tesoro_CuandoMisionIdVacio_Retorna400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/busqueda-tesoro",
            new CrearSesionBusquedaTesoroRequest(Guid.Empty));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("ValidationError");
        error.Errores.Should().ContainKey("misionId");
    }

    [Fact]
    public async Task POST_busqueda_tesoro_CuandoMisionNoExiste_Retorna404()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/busqueda-tesoro",
            new CrearSesionBusquedaTesoroRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("NotFound");
    }

    [Fact]
    public async Task POST_equipos_CuandoDatosValidos_Retorna201()
    {
        var sesionId = await CrearSesionAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/equipos",
            new RegistrarEquipoRequest("Equipo Alpha"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<RegistrarEquipoResponse>();
        body!.EquipoId.Should().NotBeEmpty();
        body.CodigoAcceso.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task POST_equipos_CuandoNombreVacio_Retorna400()
    {
        var sesionId = await CrearSesionAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/equipos",
            new RegistrarEquipoRequest(""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("ValidationError");
        error.Errores.Should().ContainKey("nombreEquipo");
    }

    [Fact]
    public async Task POST_equipos_CuandoSesionNoExiste_Retorna404()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{Guid.NewGuid()}/equipos",
            new RegistrarEquipoRequest("Equipo X"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_iniciar_CuandoSesionConEquipo_Retorna204()
    {
        var sesionId = await CrearSesionAsync();
        await RegistrarEquipoAsync(sesionId, "Equipo Beta");

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/iniciar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_iniciar_CuandoSinEquipos_Retorna400()
    {
        var sesionId = await CrearSesionAsync();

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/iniciar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task POST_pausar_CuandoSesionActiva_Retorna204()
    {
        var sesionId = await CrearSesionIniciadaAsync();

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/pausar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_pausar_CuandoSesionNoIniciada_Retorna400()
    {
        var sesionId = await CrearSesionAsync();

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/pausar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task POST_pausar_CuandoSesionNoExiste_Retorna404()
    {
        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{Guid.NewGuid()}/pausar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_reanudar_CuandoSesionPausada_Retorna204()
    {
        var sesionId = await CrearSesionIniciadaAsync();

        await _client.PostAsync($"/api/v1/sesiones/{sesionId}/pausar", null);

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/reanudar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_reanudar_CuandoSesionActiva_Retorna400()
    {
        var sesionId = await CrearSesionIniciadaAsync();

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/reanudar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task POST_reanudar_CuandoSesionNoExiste_Retorna404()
    {
        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{Guid.NewGuid()}/reanudar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Flujo_CrearRegistrarIniciar_CompletaSinError()
    {
        var misionId = await ApiTestData.SeedMisionActivaAsync(_services);

        var crear = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/busqueda-tesoro",
            new CrearSesionBusquedaTesoroRequest(misionId));
        crear.EnsureSuccessStatusCode();
        var sesionId = (await crear.Content.ReadFromJsonAsync<CrearSesionResponse>())!.Id;

        var equipo = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/equipos",
            new RegistrarEquipoRequest("Equipo Flujo"));
        equipo.EnsureSuccessStatusCode();

        var iniciar = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/iniciar",
            null);
        iniciar.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<Guid> CrearSesionAsync()
    {
        var misionId = await ApiTestData.SeedMisionActivaAsync(_services);
        var response = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/busqueda-tesoro",
            new CrearSesionBusquedaTesoroRequest(misionId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CrearSesionResponse>())!.Id;
    }

    private async Task RegistrarEquipoAsync(Guid sesionId, string nombre)
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/equipos",
            new RegistrarEquipoRequest(nombre));
        response.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CrearSesionIniciadaAsync()
    {
        var sesionId = await CrearSesionAsync();
        await RegistrarEquipoAsync(sesionId, "Equipo Pausa");
        var iniciar = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/iniciar",
            null);
        iniciar.EnsureSuccessStatusCode();
        return sesionId;
    }
}
