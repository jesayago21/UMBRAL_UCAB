using Microsoft.EntityFrameworkCore;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence;

public sealed class UmbralDbContext : DbContext
{
    public UmbralDbContext(DbContextOptions<UmbralDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sesion> Sesiones => Set<Sesion>();
    public DbSet<EquipoSesion> EquiposSesion => Set<EquipoSesion>();
    public DbSet<EventoSesion> EventosSesion => Set<EventoSesion>();
    public DbSet<Evidencia> Evidencias => Set<Evidencia>();

    public DbSet<Mision> Misiones => Set<Mision>();
    public DbSet<Etapa> EtapasMision => Set<Etapa>();
    public DbSet<Pista> PistasMision => Set<Pista>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureAssemblyMarker).Assembly);
    }
}
