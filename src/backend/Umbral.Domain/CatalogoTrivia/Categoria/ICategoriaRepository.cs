namespace Umbral.Domain.CatalogoTrivia.Categoria;

public interface ICategoriaRepository
{
    Task<Categoria?> FindByIdAsync(CategoriaId id, CancellationToken ct = default);
    Task<IReadOnlyList<Categoria>> FindAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNombreAsync(string nombre, Guid? excludeId = null, CancellationToken ct = default);
    Task SaveAsync(Categoria categoria, CancellationToken ct = default);
}
