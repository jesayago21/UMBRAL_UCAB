using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.ValueConverters;

namespace Umbral.Infrastructure.Persistence.Configurations;

internal static class ContextoMisionConfiguration
{
    public static void Configure(OwnedNavigationBuilder<Sesion, ContextoMision> ctx)
    {
        ctx.ToTable("contextos_mision");

        ctx.Property(c => c.MisionId)
            .HasColumnName("mision_id")
            .IsRequired();

        ctx.Property(c => c.EtapaActualIndex)
            .HasColumnName("etapa_actual_index")
            .IsRequired();

        ctx.Property(c => c.GanadorEtapaActualId)
            .HasColumnName("ganador_etapa_actual_id");

        ctx.Property(c => c.PreguntaTriviaActualIndex)
            .HasColumnName("pregunta_trivia_actual_index")
            .IsRequired();

        ctx.Property(c => c.MisionSnapshot)
            .HasColumnName("mision_snapshot_json")
            .HasColumnType("jsonb")
            .HasConversion(new MisionSnapshotJsonValueConverter())
            .IsRequired();

        ctx.WithOwner()
            .HasForeignKey("SesionId");
    }
}
