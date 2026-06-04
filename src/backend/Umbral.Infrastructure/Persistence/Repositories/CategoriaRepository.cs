using Microsoft.EntityFrameworkCore;
using Umbral.Domain.CatalogoTrivia.Categoria;

namespace Umbral.Infrastructure.Persistence.Repositories;

public sealed class CategoriaRepository : ICategoriaRepository
{
    private readonly UmbralDbContext _db;

    public CategoriaRepository(UmbralDbContext db) => _db = db;

    public async Task<Categoria?> FindByIdAsync(CategoriaId id, CancellationToken ct = default)
    {
        return await _db.Categorias
            .FirstOrDefaultAsync(x => x.CategoriaId == id, ct);
    }

    public async Task<IReadOnlyList<Categoria>> FindAllAsync(CancellationToken ct = default)
    {
        return await _db.Categorias
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNombreAsync(
        string nombre,
        Guid? excludeId = null,
        CancellationToken ct = default)
    {
        var normalized = nombre.Trim().ToLowerInvariant();

        var query = _db.Categorias
            .AsNoTracking()
            .Where(x => !x.Eliminada && x.Nombre.ToLower() == normalized);

        if (excludeId.HasValue)
        {
            var excluded = new CategoriaId(excludeId.Value);
            query = query.Where(x => x.CategoriaId != excluded);
        }

        return await query.AnyAsync(ct);
    }

    public async Task SaveAsync(Categoria categoria, CancellationToken ct = default)
    {
        if (_db.Entry(categoria).State == EntityState.Detached)
            await _db.Categorias.AddAsync(categoria, ct);

        await _db.SaveChangesAsync(ct);
    }
}
