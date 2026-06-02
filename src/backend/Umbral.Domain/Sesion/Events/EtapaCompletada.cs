using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

/// <summary>
/// Emitido al completar una etapa y avanzar al siguiente nodo (HU-20, RB-05).
/// </summary>
public sealed record EtapaCompletada(
    SesionId SesionId,
    int      EtapaCompletadaIndex,
    ParticipanteId ParticipanteGanadorId) : IDomainEvent
{
    public Guid     EventId     { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
