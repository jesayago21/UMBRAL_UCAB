using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Umbral.API.Contracts.Auth;
using Umbral.API.Tests.Support;

namespace Umbral.API.Tests.Controllers;

[Collection(nameof(ApiCollection))]
public sealed class RegistroParticipanteControllerTests
{
    private readonly HttpClient _client;

    public RegistroParticipanteControllerTests(ApiIntegrationFixture fixture)
        => _client = fixture.Factory.CreateClient();

    [Fact]
    public async Task POST_registroParticipante_Anonimo_Retorna201()
    {
        _client.DefaultRequestHeaders.Clear();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/registro-participante",
            new RegistroParticipanteRequest(
                $"jugador_{suffix}@test.com",
                $"jugador_{suffix}",
                "Ana",
                "Pérez",
                "Umbral123!"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegistroParticipanteResponse>();
        body!.KeycloakUserId.Should().NotBe(Guid.Empty);
        body.Username.Should().Be($"jugador_{suffix}");
    }

    [Fact]
    public async Task POST_registroParticipante_EmailDuplicado_Retorna400()
    {
        _client.DefaultRequestHeaders.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegistroParticipanteRequest(
            $"dup_part_{suffix}@test.com",
            $"dup_part_{suffix}",
            "Ana",
            "Pérez",
            "Umbral123!");

        await _client.PostAsJsonAsync("/api/v1/auth/registro-participante", request);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/registro-participante", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_registroParticipante_PasswordCorta_Retorna400()
    {
        _client.DefaultRequestHeaders.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/registro-participante",
            new RegistroParticipanteRequest(
                $"corto_{suffix}@test.com",
                $"corto_{suffix}",
                "Ana",
                "Pérez",
                "corta"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
