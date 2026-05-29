using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoTrivia.Categoria.Events;

public sealed record CategoriaCreada(CategoriaId CategoriaId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
