namespace Umbral.Domain.Shared;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OcurridoEn { get; }
}
