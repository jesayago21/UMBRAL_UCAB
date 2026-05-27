using Umbral.Domain.Ports;
using Umbral.Domain.Shared;

namespace Umbral.Infrastructure.Messaging.Publishers;

/// <summary>
/// Publicador temporal para permitir wiring de Application/Infrastructure
/// mientras se implementa el bus real de eventos.
/// </summary>
public sealed class NoOpEventPublisher : IEventPublisher
{
    public Task PublishBatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken ct = default)
        => Task.CompletedTask;
}
