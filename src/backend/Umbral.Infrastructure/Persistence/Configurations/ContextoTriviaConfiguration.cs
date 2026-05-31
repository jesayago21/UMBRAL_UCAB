using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.ValueConverters;

namespace Umbral.Infrastructure.Persistence.Configurations;

/// <summary>
/// Owned type 1:1 de <see cref="Sesion.ContextoTrivia"/> en tabla <c>contextos_trivia</c>.
/// </summary>
internal static class ContextoTriviaConfiguration
{
    public static void Configure(OwnedNavigationBuilder<Sesion, ContextoTrivia> tv)
    {
        tv.ToTable("contextos_trivia");

        tv.Property(c => c.PreguntaActualIndex)
            .HasColumnName("pregunta_actual_index")
            .IsRequired();

        tv.Property(c => c.TimerCerradoEn)
            .HasColumnName("timer_cerrado_en");

        tv.Property(c => c.PreguntasOrdenadas)
            .HasColumnName("preguntas_ordenadas_json")
            .HasColumnType("jsonb")
            .HasConversion(new PreguntasOrdenadasJsonValueConverter())
            .IsRequired();

        tv.WithOwner()
            .HasForeignKey("SesionId");
    }
}
