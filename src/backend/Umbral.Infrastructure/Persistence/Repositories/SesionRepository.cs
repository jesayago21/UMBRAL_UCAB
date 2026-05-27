using Microsoft.EntityFrameworkCore;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.Repositories;

public sealed class SesionRepository : ISesionRepository
{
    private readonly UmbralDbContext _db;

    public SesionRepository(UmbralDbContext db) => _db = db;

    public async Task<Sesion?> FindByIdAsync(SesionId id, CancellationToken ct = default)
    {
        return await _db.Sesiones
            .Include("_equipos")
            .Include("_historialEventos")
            .Include("_evidencias")
            .FirstOrDefaultAsync(x => x.SesionId == id, ct);
    }

    public async Task<IReadOnlyList<Sesion>> FindActivasAsync(CancellationToken ct = default)
    {
        return await _db.Sesiones
            .AsNoTracking()
            .Where(x => x.Estado == EstadoSesion.Activa)
            .ToListAsync(ct);
    }

    public async Task SaveAsync(Sesion sesion, CancellationToken ct = default)
    {
        if (_db.Entry(sesion).State == EntityState.Detached)
            await _db.Sesiones.AddAsync(sesion, ct);

        await _db.SaveChangesAsync(ct);
    }
}
