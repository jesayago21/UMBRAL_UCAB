using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision.Events;

public sealed record MisionCreada(MisionId MisionId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
