using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Infrastructure.Persistence.ValueConverters;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class PreguntaConfiguration : IEntityTypeConfiguration<Pregunta>
{
    public void Configure(EntityTypeBuilder<Pregunta> builder)
    {
        builder.ToTable("preguntas");

        builder.HasKey(x => x.PreguntaId);

        builder.Property(x => x.PreguntaId)
            .HasColumnName("id");

        builder.Property(x => x.Enunciado)
            .HasColumnName("enunciado")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Dificultad)
            .HasColumnName("dificultad")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CategoriaId)
            .HasColumnName("categoria_id");

        builder.Property(x => x.Eliminada)
            .HasColumnName("eliminada")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Ignore(x => x.Opciones);
        builder.Ignore(x => x.DomainEvents);

        var opcionesProperty = builder.Property<List<OpcionRespuesta>>("_opciones")
            .HasField("_opciones")
            .HasColumnName("opciones_json")
            .HasColumnType("jsonb")
            .HasConversion(new OpcionesRespuestaJsonValueConverter())
            .IsRequired();

        opcionesProperty.Metadata.SetValueComparer(new ValueComparer<List<OpcionRespuesta>>(
            (left, right) =>
                left == right
                || (left != null && right != null && left.SequenceEqual(right)),
            values => values.Aggregate(
                0,
                (hash, opcion) => HashCode.Combine(hash, opcion.GetHashCode())),
            values => values.ToList()));

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
