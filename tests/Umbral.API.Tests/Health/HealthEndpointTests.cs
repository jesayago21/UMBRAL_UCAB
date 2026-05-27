using System.Net;
using FluentAssertions;
using Umbral.API.Tests.Support;

namespace Umbral.API.Tests.Health;

[Collection(nameof(ApiCollection))]
public sealed class HealthEndpointTests
{
    private readonly HttpClient _client;

    public HealthEndpointTests(ApiIntegrationFixture fixture)
        => _client = fixture.Factory.CreateClient();

    [Fact]
    public async Task GET_health_DebeRetornar200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
