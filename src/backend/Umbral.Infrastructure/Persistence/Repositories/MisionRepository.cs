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

    public async Task<IReadOnlyList<Mision>> FindAllAsync(CancellationToken ct = default)
    {
        return await _db.Misiones
            .AsNoTracking()
            .Include("_etapas")
            .Include("_etapas._pistas")
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNombreAsync(string nombre, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalized = nombre.Trim().ToLowerInvariant();

        if (!excludeId.HasValue)
            return await _db.Misiones.AnyAsync(x => x.Nombre.ToLower() == normalized, ct);

        var excluded = new MisionId(excludeId.Value);
        return await _db.Misiones.AnyAsync(x => x.Nombre.ToLower() == normalized && x.MisionId != excluded, ct);
    }

    public async Task<bool> HasSesionesActivasAsync(MisionId misionId, CancellationToken ct = default)
    {
        var missionIdValue = misionId.Valor.ToString();
        const string sql = """
                           SELECT EXISTS(
                               SELECT 1
                               FROM sesiones s
                               INNER JOIN contextos_bt c ON c."SesionId" = s.id
                               WHERE s.estado = 'Activa'
                                 AND c.mision_snapshot_json ->> 'misionId' = {0}) AS "Value"
                           """;

        return await _db.Database.SqlQueryRaw<bool>(sql, missionIdValue).SingleAsync(ct);
    }

    public async Task SaveAsync(Mision mision, CancellationToken ct = default)
    {
        if (_db.Entry(mision).State == EntityState.Detached)
            await _db.Misiones.AddAsync(mision, ct);

        await _db.SaveChangesAsync(ct);
    }
}
