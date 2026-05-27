using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

public sealed record SesionFinalizada(SesionId SesionId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
