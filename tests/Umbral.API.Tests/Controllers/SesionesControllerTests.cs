using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Umbral.API.Auth;
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
        body.CodigoAcceso.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task POST_trivia_CuandoCategoriasConPreguntas_Retorna201()
    {
        var (categoriaId, _) = await ApiTestData.SeedCategoriaConPreguntaAsync(_services);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/trivia",
            new CrearSesionTriviaRequest([categoriaId]));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<CrearSesionResponse>();
        body!.Id.Should().NotBeEmpty();
        body.CodigoAcceso.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task POST_trivia_CuandoSinCategorias_Retorna400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/trivia",
            new CrearSesionTriviaRequest([]));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("ValidationError");
        error.Errores.Should().ContainKey("categoriaIds");
    }

    [Fact]
    public async Task GET_trivia_preguntas_CuandoParticipanteInscrito_RetornaPreguntasSinRespuestaCorrecta()
    {
        var (categoriaId, _) = await ApiTestData.SeedCategoriaConPreguntaAsync(_services);
        var crear = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/trivia",
            new CrearSesionTriviaRequest([categoriaId]));
        crear.EnsureSuccessStatusCode();
        var sesion = (await crear.Content.ReadFromJsonAsync<CrearSesionResponse>())!;

        await _client.PostAsync(
            $"/api/v1/sesiones/{sesion.Id}/abrir-inscripcion",
            null);

        var jugadorId = Guid.NewGuid();
        await UnirseParticipanteAsync(sesion.Id, sesion.CodigoAcceso, "Alpha", jugadorId);
        SetParticipanteAuth(jugadorId);

        var response = await _client.GetAsync(
            $"/api/v1/sesiones/{sesion.Id}/trivia/preguntas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var preguntas = await response.Content.ReadFromJsonAsync<List<PreguntaTriviaParticipanteResponse>>();
        preguntas.Should().NotBeNull();
        var list = preguntas!;
        list.Should().NotBeEmpty();
        list.Should().OnlyContain(p => p.Opciones.Count >= 3);
        list[0].Orden.Should().Be(1);
    }

    [Fact]
    public async Task GET_trivia_preguntas_CuandoNoInscrito_Retorna401()
    {
        var (categoriaId, _) = await ApiTestData.SeedCategoriaConPreguntaAsync(_services);
        var crear = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/trivia",
            new CrearSesionTriviaRequest([categoriaId]));
        crear.EnsureSuccessStatusCode();
        var sesion = (await crear.Content.ReadFromJsonAsync<CrearSesionResponse>())!;

        SetParticipanteAuth(Guid.NewGuid());

        var response = await _client.GetAsync(
            $"/api/v1/sesiones/{sesion.Id}/trivia/preguntas");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
    public async Task POST_abrir_inscripcion_CuandoProgramada_Retorna204()
    {
        var (sesionId, _) = await CrearSesionAsync();

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/abrir-inscripcion",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_unirse_CuandoDatosValidos_Retorna201()
    {
        var (sesionId, codigo) = await CrearSesionAsync();

        var response = await UnirseParticipanteRequestAsync(sesionId, codigo, "Alpha");

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<UnirseSesionResponse>();
        body!.ParticipanteId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task POST_unirse_CuandoCodigoAccesoVacio_Retorna400()
    {
        var (sesionId, _) = await CrearSesionAsync();

        var response = await UnirseParticipanteRequestAsync(sesionId, "", "Alpha");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("ValidationError");
        error.Errores.Should().ContainKey("codigoAcceso");
    }

    [Fact]
    public async Task POST_unirse_CuandoSesionNoExiste_Retorna404()
    {
        var response = await UnirseParticipanteRequestAsync(
            Guid.NewGuid(),
            "ABC123",
            "Alpha");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_unirse_CuandoCodigoInvalido_Retorna400()
    {
        var (sesionId, _) = await CrearSesionAsync();

        var response = await UnirseParticipanteRequestAsync(sesionId, "CODIGO-INVALIDO", "Alpha");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task POST_unirse_CuandoSesionActiva_Retorna400()
    {
        var (sesionId, codigo) = await CrearSesionAsync();
        await UnirseParticipanteAsync(sesionId, codigo, "Alpha");
        await IniciarSesionAsync(sesionId);

        var response = await UnirseParticipanteRequestAsync(sesionId, codigo, "Beta");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task POST_iniciar_CuandoSesionConParticipante_Retorna204()
    {
        var (sesionId, codigo) = await CrearSesionAsync();
        await UnirseParticipanteAsync(sesionId, codigo, "Beta");
        ResetAuth();

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/iniciar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_iniciar_CuandoSinParticipantes_Retorna400()
    {
        var (sesionId, _) = await CrearSesionAsync();

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
        var (sesionId, _) = await CrearSesionAsync();

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
        var (sesionId, participanteId) = await CrearSesionIniciadaConParticipanteAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/penalizaciones",
            new AplicarPenalizacionRequest(participanteId, 15, "Llegada tardía"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_penalizaciones_CuandoPuntosInvalidos_Retorna400()
    {
        var (sesionId, participanteId) = await CrearSesionIniciadaConParticipanteAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/penalizaciones",
            new AplicarPenalizacionRequest(participanteId, 0, "Motivo válido"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("ValidationError");
        error.Errores.Should().ContainKey("puntos");
    }

    [Fact]
    public async Task POST_penalizaciones_CuandoSesionNoIniciada_Retorna400()
    {
        var (sesionId, codigo) = await CrearSesionAsync();
        var participanteId = await UnirseParticipanteYObtenerIdAsync(sesionId, codigo, "Gamma");

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/penalizaciones",
            new AplicarPenalizacionRequest(participanteId, 10, "Fuera de tiempo"));

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
        var (sesionId, participanteId) = await CrearSesionIniciadaConParticipanteAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/evidencias",
            new SubmitEvidenciaRequest(participanteId, "QR-API-001"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<SubmitEvidenciaResponse>();
        body!.EvidenciaId.Should().NotBeEmpty();
        body.Resultado.Should().Be("Valida");
    }

    [Fact]
    public async Task POST_evidencias_CuandoCodigoQrVacio_Retorna400()
    {
        var (sesionId, participanteId) = await CrearSesionIniciadaConParticipanteAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/evidencias",
            new SubmitEvidenciaRequest(participanteId, ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("ValidationError");
        error.Errores.Should().ContainKey("codigoQr");
    }

    [Fact]
    public async Task POST_evidencias_CuandoQrInvalido_Retorna201ConResultadoInvalida()
    {
        var (sesionId, participanteId) = await CrearSesionIniciadaConParticipanteAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/evidencias",
            new SubmitEvidenciaRequest(participanteId, "QR-INEXISTENTE"));

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
    public async Task POST_finalizar_CuandoSesionActiva_Retorna204()
    {
        var sesionId = await CrearSesionIniciadaAsync();

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/finalizar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_finalizar_CuandoSesionNoIniciada_Retorna400()
    {
        var (sesionId, _) = await CrearSesionAsync();

        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/finalizar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task POST_finalizar_CuandoSesionNoExiste_Retorna404()
    {
        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{Guid.NewGuid()}/finalizar",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_cancelar_CuandoMotivoValido_Retorna204()
    {
        var sesionId = await CrearSesionIniciadaAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/cancelar",
            new CancelarSesionRequest("Clima adverso"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task POST_cancelar_CuandoMotivoVacio_Retorna400()
    {
        var sesionId = await CrearSesionIniciadaAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/cancelar",
            new CancelarSesionRequest(""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("ValidationError");
        error.Errores.Should().ContainKey("motivo");
    }

    [Fact]
    public async Task POST_cancelar_CuandoSesionYaFinalizada_Retorna400()
    {
        var sesionId = await CrearSesionIniciadaAsync();

        await _client.PostAsync($"/api/v1/sesiones/{sesionId}/finalizar", null);

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/cancelar",
            new CancelarSesionRequest("Intento tardío"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        error!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task POST_cancelar_CuandoSesionNoExiste_Retorna404()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{Guid.NewGuid()}/cancelar",
            new CancelarSesionRequest("Motivo"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ranking_CuandoEmpateOrdenaPorNombre_Retorna200()
    {
        var (sesionId, codigo) = await CrearSesionAsync();
        await UnirseParticipanteAsync(sesionId, codigo, "Gamma");
        await UnirseParticipanteAsync(sesionId, codigo, "Alpha");
        await IniciarSesionAsync(sesionId);

        var response = await _client.GetAsync($"/api/v1/sesiones/{sesionId}/ranking");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var ranking = await response.Content.ReadFromJsonAsync<List<PosicionRankingResponse>>();
        ranking.Should().HaveCount(2);
        ranking![0].NombreParticipante.Should().Be("Alpha");
        ranking[0].PuntajeTotal.Should().Be(0);
        ranking[1].NombreParticipante.Should().Be("Gamma");
    }

    [Fact]
    public async Task GET_ranking_DespuesDeEvidenciaValida_OrdenaPorPuntaje()
    {
        var (sesionId, codigo) = await CrearSesionAsync();
        var alphaId = await UnirseParticipanteYObtenerIdAsync(sesionId, codigo, "Alpha");
        var betaId = await UnirseParticipanteYObtenerIdAsync(sesionId, codigo, "Beta");
        await IniciarSesionAsync(sesionId);

        await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/evidencias",
            new SubmitEvidenciaRequest(betaId, "QR-API-001"));

        var response = await _client.GetAsync($"/api/v1/sesiones/{sesionId}/ranking");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var ranking = await response.Content.ReadFromJsonAsync<List<PosicionRankingResponse>>();
        ranking.Should().HaveCount(2);
        ranking![0].ParticipanteId.Should().Be(betaId);
        ranking[0].PuntajeTotal.Should().Be(100);
        ranking[1].ParticipanteId.Should().Be(alphaId);
        ranking[1].PuntajeTotal.Should().Be(0);
    }

    [Fact]
    public async Task GET_ranking_CuandoSesionNoExiste_Retorna404()
    {
        var response = await _client.GetAsync(
            $"/api/v1/sesiones/{Guid.NewGuid()}/ranking");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Flujo_CrearUnirseIniciar_CompletaSinError()
    {
        var misionId = await ApiTestData.SeedMisionActivaAsync(_services);

        var crear = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/busqueda-tesoro",
            new CrearSesionBusquedaTesoroRequest(misionId));
        crear.EnsureSuccessStatusCode();
        var sesion = (await crear.Content.ReadFromJsonAsync<CrearSesionResponse>())!;

        var unirse = await UnirseParticipanteRequestAsync(sesion.Id, sesion.CodigoAcceso, "Alpha");
        unirse.EnsureSuccessStatusCode();
        ResetAuth();

        var iniciar = await _client.PostAsync(
            $"/api/v1/sesiones/{sesion.Id}/iniciar",
            null);
        iniciar.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<(Guid Id, string CodigoAcceso)> CrearSesionAsync()
    {
        var misionId = await ApiTestData.SeedMisionActivaAsync(_services);
        var response = await _client.PostAsJsonAsync(
            "/api/v1/sesiones/busqueda-tesoro",
            new CrearSesionBusquedaTesoroRequest(misionId));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<CrearSesionResponse>();
        return (body!.Id, body.CodigoAcceso);
    }

    private async Task UnirseParticipanteAsync(
        Guid sesionId,
        string codigo,
        string? nombre = null,
        Guid? jugadorId = null)
    {
        var response = await UnirseParticipanteRequestAsync(sesionId, codigo, nombre, jugadorId);
        response.EnsureSuccessStatusCode();
        ResetAuth();
    }

    private async Task<HttpResponseMessage> UnirseParticipanteRequestAsync(
        Guid sesionId,
        string codigo,
        string? nombre = null,
        Guid? jugadorId = null)
    {
        SetParticipanteAuth(jugadorId ?? Guid.NewGuid());

        return await _client.PostAsJsonAsync(
            $"/api/v1/sesiones/{sesionId}/unirse",
            new UnirseSesionRequest(codigo, nombre ?? "Participante"));
    }

    private async Task<Guid> CrearSesionIniciadaAsync()
    {
        var (sesionId, codigo) = await CrearSesionAsync();
        await UnirseParticipanteAsync(sesionId, codigo, "Beta");
        await IniciarSesionAsync(sesionId);
        return sesionId;
    }

    private async Task IniciarSesionAsync(Guid sesionId)
    {
        ResetAuth();
        var response = await _client.PostAsync(
            $"/api/v1/sesiones/{sesionId}/iniciar",
            null);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task POST_sesiones_CuandoMisionActiva_Retorna201()
    {
        var misionId = await ApiTestData.SeedMisionActivaAsync(_services);

        var response = await _client.PostAsJsonAsync(
            "/api/v1/sesiones",
            new CrearSesionMisionRequest(misionId, "Sesión API test"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<CrearSesionMisionResponse>();
        body!.Id.Should().NotBeEmpty();
        body.CodigoAcceso.Should().NotBeNullOrWhiteSpace();
        body.MisionNombre.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GET_disponibles_CuandoParticipante_Retorna200()
    {
        var misionId = await ApiTestData.SeedMisionActivaAsync(_services);
        await _client.PostAsJsonAsync(
            "/api/v1/sesiones",
            new CrearSesionMisionRequest(misionId, "Sesión API test"));

        SetParticipanteAuth(Guid.NewGuid());

        var response = await _client.GetAsync("/api/v1/sesiones/disponibles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<SesionDisponibleParticipanteResponse>>();
        body.Should().NotBeNull();
        body!.Should().NotBeEmpty();
        ResetAuth();
    }

    [Fact]
    public async Task GET_sesiones_CuandoOperador_RetornaOperativas()
    {
        var (sesionId, codigo) = await CrearSesionAsync();
        await UnirseParticipanteAsync(sesionId, codigo, "Alpha");
        ResetAuth();

        var response = await _client.GetAsync("/api/v1/sesiones");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<SesionResumenResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(x => x.Id == sesionId);
        body.Should().OnlyContain(x =>
            x.Estado == "Programada"
            || x.Estado == "EnPreparacion"
            || x.Estado == "Activa"
            || x.Estado == "Pausada");
    }

    [Fact]
    public async Task GET_sesion_por_id_CuandoExiste_RetornaDetalle()
    {
        var (sesionId, codigo) = await CrearSesionAsync();
        await UnirseParticipanteAsync(sesionId, codigo, "Beta");
        ResetAuth();

        var response = await _client.GetAsync($"/api/v1/sesiones/{sesionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<SesionDetalleResponse>();
        body!.Id.Should().Be(sesionId);
        body.Participantes.Should().ContainSingle(e => e.Nombre == "Beta");
        body.TotalEtapas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GET_sesion_por_id_CuandoNoExiste_Retorna404()
    {
        var response = await _client.GetAsync($"/api/v1/sesiones/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<(Guid SesionId, Guid ParticipanteId)> CrearSesionIniciadaConParticipanteAsync()
    {
        var (sesionId, codigo) = await CrearSesionAsync();
        var participanteId = await UnirseParticipanteYObtenerIdAsync(sesionId, codigo, "Gamma");
        await IniciarSesionAsync(sesionId);
        return (sesionId, participanteId);
    }

    private async Task<Guid> UnirseParticipanteYObtenerIdAsync(
        Guid sesionId,
        string codigo,
        string nombre,
        Guid? jugadorId = null)
    {
        var response = await UnirseParticipanteRequestAsync(sesionId, codigo, nombre, jugadorId);
        response.EnsureSuccessStatusCode();
        var participanteId = (await response.Content.ReadFromJsonAsync<UnirseSesionResponse>())!.ParticipanteId;
        ResetAuth();
        return participanteId;
    }

    private void SetParticipanteAuth(Guid jugadorId)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeaderName);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, jugadorId.ToString());
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeaderName);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, "Participante");
    }

    private void ResetAuth()
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeaderName);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeaderName);
    }
}
