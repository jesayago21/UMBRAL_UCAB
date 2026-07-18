using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class ParticipanteSesionConfiguration : IEntityTypeConfiguration<ParticipanteSesion>
{
    public void Configure(EntityTypeBuilder<ParticipanteSesion> builder)
    {
        builder.ToTable("participantes_sesion");

        builder.HasKey(x => x.ParticipanteId);

        builder.Property(x => x.ParticipanteId)
            .HasColumnName("id");

        builder.Property(x => x.SesionId)
            .HasColumnName("sesion_id")
            .IsRequired();

        builder.Property(x => x.JugadorId)
            .HasColumnName("jugador_id")
            .IsRequired();

        builder.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasConversion(x => x.Valor, value => NombreParticipante.Crear(value))
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.PuntajeTotal)
            .HasColumnName("puntaje_total")
            .HasConversion(x => x.Valor, value => Puntaje.Crear(value))
            .IsRequired();

        builder.Property(x => x.DeudaPendiente)
            .HasColumnName("deuda_pendiente")
            .HasConversion(x => x.Valor, value => Puntaje.Crear(value))
            .IsRequired()
            .HasDefaultValueSql("0");

        builder.Property(x => x.TiempoBusquedaMs)
            .HasColumnName("tiempo_busqueda_ms")
            .IsRequired()
            .HasDefaultValueSql("0");

        builder.HasOne<Sesion>()
            .WithMany("_participantes")
            .HasForeignKey(x => x.SesionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SesionId, x.Nombre })
            .IsUnique();

        builder.HasIndex(x => new { x.SesionId, x.JugadorId })
            .IsUnique();
    }
}
