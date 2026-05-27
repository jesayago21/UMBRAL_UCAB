using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class MisionConfiguration : IEntityTypeConfiguration<Mision>
{
    public void Configure(EntityTypeBuilder<Mision> builder)
    {
        builder.ToTable("misiones");

        builder.HasKey(x => x.MisionId);

        builder.Property(x => x.MisionId)
            .HasColumnName("id");

        builder.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Ignore(x => x.Etapas);
        builder.Ignore(x => x.DomainEvents);

        builder.HasMany<Etapa>("_etapas")
            .WithOne()
            .HasForeignKey(x => x.MisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
