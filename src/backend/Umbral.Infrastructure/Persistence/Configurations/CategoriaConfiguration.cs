using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");

        builder.HasKey(x => x.CategoriaId);

        builder.Property(x => x.CategoriaId)
            .HasColumnName("id");

        builder.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Eliminada)
            .HasColumnName("eliminada")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(x => x.Nombre)
            .IsUnique()
            .HasFilter("eliminada = false");

        builder.Ignore(x => x.DomainEvents);
    }
}
