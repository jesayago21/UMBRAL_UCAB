using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class RespuestaTriviaConfiguration : IEntityTypeConfiguration<RespuestaTrivia>
{
    public void Configure(EntityTypeBuilder<RespuestaTrivia> builder)
    {
        builder.ToTable("respuestas_trivia");

        builder.HasKey(x => x.RespuestaTriviaId);

        builder.Property(x => x.RespuestaTriviaId)
            .HasColumnName("id");

        builder.Property(x => x.SesionId)
            .HasColumnName("sesion_id")
            .IsRequired();

        builder.Property(x => x.ParticipanteId)
            .HasColumnName("participante_id")
            .IsRequired();

        builder.Property(x => x.PreguntaId)
            .HasColumnName("pregunta_id")
            .IsRequired();

        builder.Property(x => x.IndiceOpcion)
            .HasColumnName("indice_opcion")
            .IsRequired();

        builder.Property(x => x.TimestampServidor)
            .HasColumnName("timestamp_servidor")
            .IsRequired();

        builder.Property(x => x.FueraDeTiempo)
            .HasColumnName("fuera_de_tiempo")
            .IsRequired();

        builder.Property(x => x.EsCorrecta)
            .HasColumnName("es_correcta")
            .IsRequired();

        builder.Property(x => x.PuntosOtorgados)
            .HasColumnName("puntos_otorgados")
            .IsRequired();

        builder.Property(x => x.TiempoRespuestaMs)
            .HasColumnName("tiempo_respuesta_ms")
            .IsRequired();

        builder.HasIndex(x => new { x.SesionId, x.ParticipanteId, x.PreguntaId })
            .IsUnique()
            .HasDatabaseName("IX_respuestas_trivia_sesion_participante_pregunta");

        builder.HasOne<Sesion>()
            .WithMany("_respuestasTrivia")
            .HasForeignKey(x => x.SesionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
