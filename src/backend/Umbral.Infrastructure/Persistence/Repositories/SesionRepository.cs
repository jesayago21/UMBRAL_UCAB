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
            .AsNoTracking()
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
        var exists = await _db.Sesiones.AnyAsync(s => s.SesionId == sesion.SesionId, ct);

        if (!exists)
        {
            await _db.Sesiones.AddAsync(sesion, ct);
            await _db.SaveChangesAsync(ct);
            return;
        }

        await _db.Sesiones
            .Where(s => s.SesionId == sesion.SesionId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(s => s.Estado, sesion.Estado)
                    .SetProperty(s => s.IniciadaEn, sesion.IniciadaEn)
                    .SetProperty(s => s.FinalizadaEn, sesion.FinalizadaEn),
                ct);

        await InsertNewChildrenAsync(sesion, ct);
        await _db.SaveChangesAsync(ct);
    }

    private async Task InsertNewChildrenAsync(Sesion sesion, CancellationToken ct)
    {
        foreach (var equipo in sesion.Equipos)
        {
            var exists = await _db.EquiposSesion
                .AnyAsync(e => e.EquipoId == equipo.EquipoId, ct);

            if (!exists)
                await _db.EquiposSesion.AddAsync(equipo, ct);
        }

        foreach (var evento in sesion.HistorialEventos)
        {
            var exists = await _db.EventosSesion
                .AnyAsync(e => e.EventoId == evento.EventoId, ct);

            if (!exists)
                await _db.EventosSesion.AddAsync(evento, ct);
        }

        foreach (var evidencia in sesion.Evidencias)
        {
            var exists = await _db.Evidencias
                .AnyAsync(e => e.EvidenciaId == evidencia.EvidenciaId, ct);

            if (!exists)
                await _db.Evidencias.AddAsync(evidencia, ct);
        }
    }
}
