using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Umbral.API.Contracts.Auth;
using Umbral.API.Tests.Support;

namespace Umbral.API.Tests.Controllers;

[Collection(nameof(ApiCollection))]
public sealed class AuthControllerTests
{
    private readonly HttpClient _client;

    public AuthControllerTests(ApiIntegrationFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task POST_login_CredencialesValidas_RetornaToken()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("operador@umbral.local", "Umbral123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Role.Should().Be("Operador");
        body.TokenType.Should().Be("Bearer");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(body.AccessToken);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "operador@umbral.local");
    }

    [Fact]
    public async Task POST_login_PasswordInvalida_Retorna401()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("operador@umbral.local", "incorrecta"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
