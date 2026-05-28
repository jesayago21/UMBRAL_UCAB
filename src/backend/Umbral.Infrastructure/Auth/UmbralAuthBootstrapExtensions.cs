using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Umbral.Infrastructure.Persistence;

namespace Umbral.Infrastructure.Auth;

public static class UmbralAuthBootstrapExtensions
{
    public static async Task SeedDemoUsersAsync(this IServiceProvider services, bool seedEnabled)
    {
        if (!seedEnabled)
            return;

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UmbralDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoUsersSeeder>();

        await dbContext.Database.MigrateAsync();
        await seeder.SeedAsync(dbContext);
    }
}
