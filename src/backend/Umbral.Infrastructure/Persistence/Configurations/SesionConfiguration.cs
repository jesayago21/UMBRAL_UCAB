using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> builder)
    {
        builder.ToTable("sesiones");

        builder.HasKey(x => x.SesionId);

        builder.Property(x => x.SesionId)
            .HasColumnName("id");

        builder.Property(x => x.TipoSesion)
            .HasColumnName("tipo_sesion")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.OperadorId)
            .HasColumnName("operador_id")
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.IniciadaEn)
            .HasColumnName("iniciada_en")
            .IsRequired();

        builder.Property(x => x.FinalizadaEn)
            .HasColumnName("finalizada_en");

        builder.OwnsOne(
            x => x.ContextoBT,
            ContextoBusquedaTesoroConfiguration.Configure);

        builder.Navigation(x => x.ContextoBT).IsRequired(false);

        builder.Ignore(x => x.Equipos);
        builder.Ignore(x => x.HistorialEventos);
        builder.Ignore(x => x.Evidencias);
        builder.Ignore(x => x.DomainEvents);
    }
}
