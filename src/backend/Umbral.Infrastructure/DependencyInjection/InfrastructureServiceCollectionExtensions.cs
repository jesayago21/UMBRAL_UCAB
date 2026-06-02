using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Identidad;
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
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IEventPublisher, NoOpEventPublisher>();

        services.AddOptions<KeycloakAdminOptions>()
            .Bind(configuration.GetSection(KeycloakAdminOptions.SectionName));
        services.AddHttpClient<KeycloakIdentityService>();
        services.AddScoped<IIdentityService>(sp => sp.GetRequiredService<KeycloakIdentityService>());
        services.AddScoped<UsuariosEspejoSeeder>();

        return services;
    }
}
