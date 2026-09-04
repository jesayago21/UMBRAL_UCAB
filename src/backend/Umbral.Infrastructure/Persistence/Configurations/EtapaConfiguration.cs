using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Infrastructure.Persistence.ValueConverters;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class EtapaConfiguration : IEntityTypeConfiguration<Etapa>
{
    public void Configure(EntityTypeBuilder<Etapa> builder)
    {
        builder.ToTable("etapas");

        builder.HasDiscriminator<string>("tipo_etapa")
            .HasValue<EtapaBusquedaTesoro>(nameof(TipoEtapa.BusquedaTesoro))
            .HasValue<EtapaTrivia>(nameof(TipoEtapa.Trivia));

        builder.HasKey(x => x.EtapaId);

        builder.Property(x => x.EtapaId)
            .HasColumnName("id");

        builder.Property(x => x.MisionId)
            .HasColumnName("mision_id")
            .IsRequired();

        builder.Property(x => x.Orden)
            .HasColumnName("orden")
            .IsRequired();

        builder.HasIndex(x => new { x.MisionId, x.Orden })
            .IsUnique();

        builder.Ignore(x => x.Tipo);
    }
}

public sealed class EtapaBusquedaTesoroConfiguration : IEntityTypeConfiguration<EtapaBusquedaTesoro>
{
    public void Configure(EntityTypeBuilder<EtapaBusquedaTesoro> builder)
    {
        builder.Property(x => x.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CodigoQRSolucion)
            .HasColumnName("codigo_qr_solucion")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Latitud)
            .HasColumnName("latitud");

        builder.Property(x => x.Longitud)
            .HasColumnName("longitud");

        builder.Property(x => x.RadioMetros)
            .HasColumnName("radio_metros");

        builder.Ignore(x => x.Pistas);

        builder.HasMany<Pista>("_pistas")
            .WithOne()
            .HasForeignKey(x => x.EtapaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class EtapaTriviaConfiguration : IEntityTypeConfiguration<EtapaTrivia>
{
    public void Configure(EntityTypeBuilder<EtapaTrivia> builder)
    {
        builder.Property(x => x.CategoriaIdsStorage)
            .HasColumnName("categoria_ids_json")
            .HasColumnType("jsonb")
            .HasConversion(new CategoriaIdsJsonValueConverter())
            .Metadata.SetValueComparer(CategoriaIdsJsonValueConverter.Comparer);

        builder.Ignore(x => x.CategoriaIds);
    }
}
