using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision.Events;

public sealed record MisionActivada(MisionId MisionId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
