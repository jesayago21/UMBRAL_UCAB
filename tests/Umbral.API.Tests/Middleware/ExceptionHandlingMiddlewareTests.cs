using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Umbral.API.Tests.Support;

namespace Umbral.API.Tests.Middleware;

[Collection(nameof(ApiCollection))]
public sealed class ExceptionHandlingMiddlewareTests
{
    private readonly HttpClient _client;

    public ExceptionHandlingMiddlewareTests(ApiIntegrationFixture fixture)
        => _client = fixture.Factory.CreateClient();

    [Fact]
    public async Task GET_validation_DebeRetornar400ConContratoEstandar()
    {
        var response = await _client.GetAsync("/__test/errors/validation");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        body.Should().NotBeNull();
        body!.Tipo.Should().Be("ValidationError");
        body.Mensaje.Should().Contain("validación");
        body.Errores.Should().ContainKey("misionId");
        body.Errores!["misionId"].Should().Contain("El identificador de la misión es obligatorio.");
        body.TraceId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GET_notfound_DebeRetornar404()
    {
        var response = await _client.GetAsync("/__test/errors/notfound");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        body!.Tipo.Should().Be("NotFound");
    }

    [Fact]
    public async Task GET_domain_DebeRetornar400DomainError()
    {
        var response = await _client.GetAsync("/__test/errors/domain");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        body!.Tipo.Should().Be("DomainError");
    }

    [Fact]
    public async Task GET_internal_DebeRetornar500SinDetalle()
    {
        var response = await _client.GetAsync("/__test/errors/internal");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponseDto>();
        body!.Tipo.Should().Be("InternalServerError");
        body.Mensaje.Should().NotContain("simulado");
    }
}
