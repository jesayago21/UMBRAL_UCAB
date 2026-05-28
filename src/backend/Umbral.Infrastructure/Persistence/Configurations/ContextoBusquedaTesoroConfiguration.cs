using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.ValueConverters;

namespace Umbral.Infrastructure.Persistence.Configurations;

/// <summary>
/// Owned type 1:1 de <see cref="Sesion.ContextoBT"/> en tabla <c>contextos_bt</c>.
/// </summary>
internal static class ContextoBusquedaTesoroConfiguration
{
    public static void Configure(OwnedNavigationBuilder<Sesion, ContextoBusquedaTesoro> bt)
    {
        bt.ToTable("contextos_bt");

        bt.Property(c => c.EtapaActualIndex)
            .HasColumnName("etapa_actual_index")
            .IsRequired();

        bt.Property(c => c.GanadorEtapaActualId)
            .HasColumnName("ganador_etapa_actual_id");

        bt.Property(c => c.MisionSnapshot)
            .HasColumnName("mision_snapshot_json")
            .HasColumnType("jsonb")
            .HasConversion(new MisionSnapshotJsonValueConverter())
            .IsRequired();

        bt.WithOwner()
            .HasForeignKey("SesionId");
    }
}
