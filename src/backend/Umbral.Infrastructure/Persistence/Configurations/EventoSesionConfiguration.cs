using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class EventoSesionConfiguration : IEntityTypeConfiguration<EventoSesion>
{
    public void Configure(EntityTypeBuilder<EventoSesion> builder)
    {
        builder.ToTable("eventos_sesion");

        builder.HasKey(x => x.EventoId);

        builder.Property(x => x.EventoId)
            .HasColumnName("id");

        builder.Property(x => x.SesionId)
            .HasColumnName("sesion_id")
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.OcurridoEn)
            .HasColumnName("ocurrido_en")
            .IsRequired();

        builder.HasOne<Sesion>()
            .WithMany("_historialEventos")
            .HasForeignKey(x => x.SesionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SesionId, x.OcurridoEn });
    }
}
