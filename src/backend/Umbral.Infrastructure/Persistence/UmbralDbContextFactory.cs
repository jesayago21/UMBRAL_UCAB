using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Umbral.Infrastructure.Persistence;

/// <summary>
/// Factoría usada solo por la CLI de EF Core en tiempo de diseño (migraciones).
/// No participa en el runtime; se excluye de cobertura (umbral-quality-spec §13).
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class UmbralDbContextFactory : IDesignTimeDbContextFactory<UmbralDbContext>
{
    public UmbralDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5433;Database=umbral_db;Username=umbral_user;Password=umbral_pass";

        var optionsBuilder = new DbContextOptionsBuilder<UmbralDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new UmbralDbContext(optionsBuilder.Options);
    }
}
