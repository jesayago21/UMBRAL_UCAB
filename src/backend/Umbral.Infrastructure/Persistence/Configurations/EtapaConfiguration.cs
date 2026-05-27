using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class EtapaConfiguration : IEntityTypeConfiguration<Etapa>
{
    public void Configure(EntityTypeBuilder<Etapa> builder)
    {
        builder.ToTable("etapas");

        builder.HasKey(x => x.EtapaId);

        builder.Property(x => x.EtapaId)
            .HasColumnName("id");

        builder.Property(x => x.MisionId)
            .HasColumnName("mision_id")
            .IsRequired();

        builder.Property(x => x.Orden)
            .HasColumnName("orden")
            .IsRequired();

        builder.Property(x => x.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CodigoQRSolucion)
            .HasColumnName("codigo_qr_solucion")
            .HasMaxLength(120)
            .IsRequired();

        builder.Ignore(x => x.Pistas);

        builder.HasMany<Pista>("_pistas")
            .WithOne()
            .HasForeignKey(x => x.EtapaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.MisionId, x.Orden })
            .IsUnique();
    }
}
