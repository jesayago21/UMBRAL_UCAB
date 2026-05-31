using Umbral.Domain.CatalogoTrivia.Categoria;

namespace Umbral.Domain.CatalogoTrivia.Pregunta;

public interface IPreguntaRepository
{
    Task<Pregunta?> FindByIdAsync(PreguntaId id, CancellationToken ct = default);
    Task<IReadOnlyList<Pregunta>> FindByIdsAsync(IReadOnlyList<PreguntaId> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Pregunta>> FindAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Pregunta>> FindByCategoriaAsync(CategoriaId categoriaId, CancellationToken ct = default);
    Task SaveAsync(Pregunta pregunta, CancellationToken ct = default);
}
