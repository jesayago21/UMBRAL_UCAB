using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

public sealed record SesionCancelada(
    SesionId SesionId,
    string   Motivo) : IDomainEvent
{
    public Guid     EventId     { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
