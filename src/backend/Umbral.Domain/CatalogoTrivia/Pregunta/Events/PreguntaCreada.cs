using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoTrivia.Pregunta.Events;

public sealed record PreguntaCreada(PreguntaId PreguntaId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
