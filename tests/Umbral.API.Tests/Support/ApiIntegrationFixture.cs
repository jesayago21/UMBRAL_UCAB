using Testcontainers.PostgreSql;

namespace Umbral.API.Tests.Support;

public sealed class ApiIntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public UmbralWebAppFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        Factory = new UmbralWebAppFactory(_postgres.GetConnectionString());
        await Factory.MigrateDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
