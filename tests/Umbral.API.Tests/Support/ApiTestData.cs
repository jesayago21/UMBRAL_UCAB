using Microsoft.Extensions.DependencyInjection;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Infrastructure.Persistence;
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
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-API-001");
        mision.AgregarEtapaBusquedaTesoro("Etapa 2", "QR-API-002");
        mision.Activar();
        mision.ClearDomainEvents();

        await repo.SaveAsync(mision);
        return mision.MisionId.Valor;
    }

    public static async Task<(Guid CategoriaId, Guid PreguntaId)> SeedCategoriaConPreguntaAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db            = scope.ServiceProvider.GetRequiredService<UmbralDbContext>();
        var categoriaRepo = new CategoriaRepository(db);
        var preguntaRepo  = new PreguntaRepository(db);

        var categoria = Categoria.Crear($"Categoría API {Guid.NewGuid():N}");
        categoria.ClearDomainEvents();
        await categoriaRepo.SaveAsync(categoria);

        var pregunta = Pregunta.Crear(
            $"Pregunta integración API {Guid.NewGuid():N}",
            Dificultad.Facil,
            [
                OpcionRespuesta.Crear("Correcta API", true),
                OpcionRespuesta.Crear("Incorrecta 1 API", false),
                OpcionRespuesta.Crear("Incorrecta 2 API", false)
            ],
            categoria.CategoriaId);
        pregunta.ClearDomainEvents();

        await preguntaRepo.SaveAsync(pregunta);
        return (categoria.CategoriaId.Valor, pregunta.PreguntaId.Valor);
    }
}
