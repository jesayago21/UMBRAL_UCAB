using Microsoft.Extensions.DependencyInjection;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Infrastructure.Persistence.Repositories;

namespace Umbral.API.Tests.Support;

internal static class ApiTestData
{
    public static async Task<Guid> SeedMisionActivaAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var repo = new MisionRepository(
            scope.ServiceProvider.GetRequiredService<Umbral.Infrastructure.Persistence.UmbralDbContext>());

        var mision = Mision.Crear($"Misión integración API {Guid.NewGuid():N}");
        mision.AgregarEtapa("Etapa 1", "QR-API-001");
        mision.AgregarEtapa("Etapa 2", "QR-API-002");
        mision.Activar();
        mision.ClearDomainEvents();

        await repo.SaveAsync(mision);
        return mision.MisionId.Valor;
    }
}
