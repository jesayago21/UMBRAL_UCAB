using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Umbral.Infrastructure.Persistence;

public sealed class UmbralDbContextFactory : IDesignTimeDbContextFactory<UmbralDbContext>
{
    public UmbralDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=umbral_db;Username=umbral_user;Password=umbral_pass";

        var optionsBuilder = new DbContextOptionsBuilder<UmbralDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new UmbralDbContext(optionsBuilder.Options);
    }
}
