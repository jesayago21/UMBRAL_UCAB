using Umbral.Domain.Shared;

namespace Umbral.Domain.Ports;

public interface IEventPublisher
{
    Task PublishBatchAsync(
        IReadOnlyList<IDomainEvent> events,
        CancellationToken ct = default);
}
