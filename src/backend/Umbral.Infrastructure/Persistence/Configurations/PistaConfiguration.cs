using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class PistaConfiguration : IEntityTypeConfiguration<Pista>
{
    public void Configure(EntityTypeBuilder<Pista> builder)
    {
        builder.ToTable("pistas");

        builder.HasKey(x => x.PistaId);

        builder.Property(x => x.PistaId)
            .HasColumnName("id");

        builder.Property(x => x.EtapaId)
            .HasColumnName("etapa_id")
            .IsRequired();

        builder.Property(x => x.Contenido)
            .HasColumnName("contenido")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.TipoLiberacion)
            .HasColumnName("tipo_liberacion")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SegundosLiberacion)
            .HasColumnName("segundos_liberacion");
    }
}
