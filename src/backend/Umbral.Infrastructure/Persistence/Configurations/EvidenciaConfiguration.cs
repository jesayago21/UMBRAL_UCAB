using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class EvidenciaConfiguration : IEntityTypeConfiguration<Evidencia>
{
    public void Configure(EntityTypeBuilder<Evidencia> builder)
    {
        builder.ToTable("evidencias");

        builder.HasKey(x => x.EvidenciaId);

        builder.Property(x => x.EvidenciaId)
            .HasColumnName("id");

        builder.Property(x => x.SesionId)
            .HasColumnName("sesion_id")
            .IsRequired();

        builder.Property(x => x.ParticipanteId)
            .HasColumnName("participante_id")
            .IsRequired();

        builder.Property(x => x.EtapaId)
            .HasColumnName("etapa_id")
            .IsRequired();

        builder.Property(x => x.CodigoQR)
            .HasColumnName("codigo_qr_leido")
            .HasConversion(x => x.Valor, value => CodigoQR.Crear(value))
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.TimestampServidor)
            .HasColumnName("timestamp_servidor")
            .IsRequired();

        builder.Property(x => x.Resultado)
            .HasColumnName("resultado")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne<Sesion>()
            .WithMany("_evidencias")
            .HasForeignKey(x => x.SesionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
