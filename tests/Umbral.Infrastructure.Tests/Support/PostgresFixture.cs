using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Umbral.Infrastructure.Persistence;

namespace Umbral.Infrastructure.Tests.Support;

public sealed class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string ConnectionString =>
        _container?.GetConnectionString()
        ?? throw new InvalidOperationException("PostgreSQL container is not started.");

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public UmbralDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<UmbralDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new UmbralDbContext(options);
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}
