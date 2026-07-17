using Microsoft.EntityFrameworkCore;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.ValueConverters;

namespace Umbral.Infrastructure.Persistence;

public sealed class UmbralDbContext : DbContext
{
    public UmbralDbContext(DbContextOptions<UmbralDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<ParticipanteSesion> ParticipantesSesion => Set<ParticipanteSesion>();
    public DbSet<EventoSesion> EventosSesion => Set<EventoSesion>();
    public DbSet<Evidencia> Evidencias => Set<Evidencia>();
    public DbSet<RespuestaTrivia> RespuestasTrivia => Set<RespuestaTrivia>();

    public DbSet<Mision> Misiones => Set<Mision>();
    public DbSet<Etapa> EtapasMision => Set<Etapa>();
    public DbSet<Pista> PistasMision => Set<Pista>();

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Pregunta> Preguntas => Set<Pregunta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureAssemblyMarker).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<MisionId>()
            .HaveConversion<MisionIdValueConverter>();
        configurationBuilder.Properties<EtapaId>()
            .HaveConversion<EtapaIdValueConverter>();
        configurationBuilder.Properties<PistaId>()
            .HaveConversion<PistaIdValueConverter>();

        configurationBuilder.Properties<SesionId>()
            .HaveConversion<SesionIdValueConverter>();
        configurationBuilder.Properties<ParticipanteId>()
            .HaveConversion<ParticipanteIdValueConverter>();
        configurationBuilder.Properties<EvidenciaId>()
            .HaveConversion<EvidenciaIdValueConverter>();
        configurationBuilder.Properties<RespuestaTriviaId>()
            .HaveConversion<RespuestaTriviaIdValueConverter>();
        configurationBuilder.Properties<UsuarioId>()
            .HaveConversion<UsuarioIdValueConverter>();
        configurationBuilder.Properties<CategoriaId>()
            .HaveConversion<CategoriaIdValueConverter>();
        configurationBuilder.Properties<PreguntaId>()
            .HaveConversion<PreguntaIdValueConverter>();
    }
}
