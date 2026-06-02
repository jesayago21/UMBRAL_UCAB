namespace Umbral.Domain.CatalogoMision.Mision;

public interface IMisionRepository
{
    Task<Mision?> FindByIdAsync(MisionId id, CancellationToken ct = default);
    Task<IReadOnlyList<Mision>> FindAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Mision>> FindActivasAsync(CancellationToken ct = default);
    Task<bool> ExistsByNombreAsync(string nombre, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> HasSesionesActivasAsync(MisionId misionId, CancellationToken ct = default);
    Task<bool> HasSesionesAsociadasAsync(MisionId misionId, CancellationToken ct = default);
    Task SaveAsync(Mision mision, CancellationToken ct = default);
    Task DeleteAsync(Mision mision, CancellationToken ct = default);
}
