using Microsoft.EntityFrameworkCore;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Infrastructure.Persistence.Repositories;

public sealed class MisionRepository : IMisionRepository
{
    private readonly UmbralDbContext _db;

    public MisionRepository(UmbralDbContext db) => _db = db;

    public async Task<Mision?> FindByIdAsync(MisionId id, CancellationToken ct = default)
    {
        return await _db.Misiones
            .Include("_etapas")
            .Include("_etapas._pistas")
            .FirstOrDefaultAsync(x => x.MisionId == id, ct);
    }

    public async Task<IReadOnlyList<Mision>> FindActivasAsync(CancellationToken ct = default)
    {
        return await _db.Misiones
            .AsNoTracking()
            .Where(x => x.Estado == EstadoMision.Activa)
            .ToListAsync(ct);
    }

    public async Task SaveAsync(Mision mision, CancellationToken ct = default)
    {
        if (_db.Entry(mision).State == EntityState.Detached)
            await _db.Misiones.AddAsync(mision, ct);

        await _db.SaveChangesAsync(ct);
    }
}
