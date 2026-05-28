using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbral.Infrastructure.Persistence;

namespace Umbral.API.Tests.Support;

public sealed class UmbralWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public UmbralWebAppFactory(string connectionString)
        => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _connectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<UmbralDbContext>));

            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<UmbralDbContext>(options =>
                options.UseNpgsql(_connectionString));
        });
    }

    public async Task MigrateDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UmbralDbContext>();
        await db.Database.MigrateAsync();
    }
}
