namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

public interface IMisionRepository
{
    Task<Mision?> FindByIdAsync(MisionId id, CancellationToken ct = default);
    Task<IReadOnlyList<Mision>> FindAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Mision>> FindActivasAsync(CancellationToken ct = default);
    Task SaveAsync(Mision mision, CancellationToken ct = default);
}
