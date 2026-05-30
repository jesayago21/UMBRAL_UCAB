using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Messaging.Publishers;
using Umbral.Infrastructure.Persistence;
using Umbral.Infrastructure.Persistence.Repositories;

namespace Umbral.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing connection string 'Postgres'.");

        services.AddDbContext<UmbralDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISesionRepository, SesionRepository>();
        services.AddScoped<IMisionRepository, MisionRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IPreguntaRepository, PreguntaRepository>();
        services.AddScoped<IEventPublisher, NoOpEventPublisher>();

        return services;
    }
}
