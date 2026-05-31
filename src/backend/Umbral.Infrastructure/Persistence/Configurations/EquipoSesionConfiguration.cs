using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class EquipoSesionConfiguration : IEntityTypeConfiguration<EquipoSesion>
{
    public void Configure(EntityTypeBuilder<EquipoSesion> builder)
    {
        builder.ToTable("equipos_sesion");

        builder.HasKey(x => x.EquipoId);

        builder.Property(x => x.EquipoId)
            .HasColumnName("id");

        builder.Property(x => x.SesionId)
            .HasColumnName("sesion_id")
            .IsRequired();

        builder.Property(x => x.JugadorId)
            .HasColumnName("jugador_id")
            .IsRequired();

        builder.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasConversion(x => x.Valor, value => NombreEquipo.Crear(value))
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.PuntajeTotal)
            .HasColumnName("puntaje_total")
            .HasConversion(x => x.Valor, value => Puntaje.Crear(value))
            .IsRequired();

        builder.HasOne<Sesion>()
            .WithMany("_equipos")
            .HasForeignKey(x => x.SesionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SesionId, x.Nombre })
            .IsUnique();

        builder.HasIndex(x => new { x.SesionId, x.JugadorId })
            .IsUnique();
    }
}
