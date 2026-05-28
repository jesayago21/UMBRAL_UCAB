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
    public async Task POST_penalizaciones_CuandoSesionActiva_Retorna204()
    {
        var (sesionId, equipoId) = await CrearSesionIniciadaConEquipoAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/penalizaciones",
            new AplicarPenalizacionRequest(equipoId, 15, "Llegada tardía"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_penalizaciones_CuandoPuntosInvalidos_Retorna400()
    {
        var (sesionId, equipoId) = await CrearSesionIniciadaConEquipoAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/penalizaciones",
            new AplicarPenalizacionRequest(equipoId, 0, "Motivo válido"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("ValidationError");
        error.Errores.Should().ContainKey("puntos");
    }

    [Fact]
    public async Task POST_penalizaciones_CuandoSesionNoIniciada_Retorna400()
    {
        var sesionId = await CrearSesionAsync();
        var equipoId = await RegistrarEquipoYObtenerIdAsync(sesionId, "Equipo Penal");

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/penalizaciones",
            new AplicarPenalizacionRequest(equipoId, 10, "Fuera de tiempo"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task POST_penalizaciones_CuandoSesionNoExiste_Retorna404()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{Guid.NewGuid()}/penalizaciones",
            new AplicarPenalizacionRequest(Guid.NewGuid(), 10, "Motivo"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_evidencias_CuandoQrValido_Retorna201()
    {
        var (sesionId, equipoId) = await CrearSesionIniciadaConEquipoAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/evidencias",
            new SubmitEvidenciaRequest(equipoId, "QR-API-001"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<SubmitEvidenciaResponse>();
        body!.EvidenciaId.Should().NotBeEmpty();
        body.Resultado.Should().Be("Valida");
    }

    [Fact]
    public async Task POST_evidencias_CuandoCodigoQrVacio_Retorna400()
    {
        var (sesionId, equipoId) = await CrearSesionIniciadaConEquipoAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/evidencias",
            new SubmitEvidenciaRequest(equipoId, ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("ValidationError");
        error.Errores.Should().ContainKey("codigoQr");
    }

    [Fact]
    public async Task POST_evidencias_CuandoQrInvalido_Retorna201ConResultadoInvalida()
    {
        var (sesionId, equipoId) = await CrearSesionIniciadaConEquipoAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/evidencias",
            new SubmitEvidenciaRequest(equipoId, "QR-INEXISTENTE"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<SubmitEvidenciaResponse>();
        body!.Resultado.Should().Be("Invalida");
    }

    [Fact]
    public async Task POST_evidencias_CuandoSesionNoExiste_Retorna404()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{Guid.NewGuid()}/evidencias",
            new SubmitEvidenciaRequest(Guid.NewGuid(), "QR-API-001"));

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

    private async Task<(Guid SesionId, Guid EquipoId)> CrearSesionIniciadaConEquipoAsync()
    {
        var sesionId = await CrearSesionAsync();
        var equipoId = await RegistrarEquipoYObtenerIdAsync(sesionId, "Equipo Juego");
        var iniciar = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/iniciar",
            null);
        iniciar.EnsureSuccessStatusCode();
        return (sesionId, equipoId);
    }

    private async Task<Guid> RegistrarEquipoYObtenerIdAsync(Guid sesionId, string nombre)
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/equipos",
            new RegistrarEquipoRequest(nombre));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RegistrarEquipoResponse>())!.EquipoId;
    }
}
