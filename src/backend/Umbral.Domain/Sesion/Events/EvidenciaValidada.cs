using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

/// <summary>
/// Emitido cuando un equipo gana la etapa con evidencia válida (HU-19, RB-04).
/// </summary>
public sealed record EvidenciaValidada(
    SesionId  SesionId,
    EquipoId  EquipoGanadorId,
    EtapaId   EtapaId,
    Puntaje   PuntosOtorgados) : IDomainEvent
{
    public Guid     EventId     { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
