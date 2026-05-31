using Microsoft.EntityFrameworkCore;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.Serialization;

namespace Umbral.Infrastructure.Persistence.Repositories;

public sealed class SesionRepository : ISesionRepository
{
    private readonly UmbralDbContext _db;

    public SesionRepository(UmbralDbContext db) => _db = db;

    public async Task<Sesion?> FindByIdAsync(SesionId id, CancellationToken ct = default)
    {
        return await _db.Sesiones
            .AsNoTracking()
            .Include(s => s.ContextoBT)
            .Include(s => s.ContextoTrivia)
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

    public async Task<IReadOnlyList<Sesion>> FindOperativasByOperadorAsync(
        UsuarioId operadorId,
        CancellationToken ct = default)
    {
        return await _db.Sesiones
            .AsNoTracking()
            .Include(s => s.ContextoBT)
            .Include(s => s.ContextoTrivia)
            .Include("_equipos")
            .Where(x =>
                x.OperadorId == operadorId &&
                x.Estado != EstadoSesion.Finalizada &&
                x.Estado != EstadoSesion.Cancelada)
            .OrderByDescending(x => x.IniciadaEn)
            .ThenByDescending(x => x.SesionId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Sesion>> FindDisponiblesParaEquipoAsync(
        TipoSesion tipo,
        CancellationToken ct = default)
    {
        return await _db.Sesiones
            .AsNoTracking()
            .Include(s => s.ContextoBT)
            .Include(s => s.ContextoTrivia)
            .Include("_equipos")
            .Where(x =>
                x.TipoSesion == tipo &&
                x.Estado == EstadoSesion.EnPreparacion)
            .OrderByDescending(x => x.SesionId)
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

        await SyncContextoBtAsync(sesion, ct);
        await SyncContextoTriviaAsync(sesion, ct);
        await InsertNewChildrenAsync(sesion, ct);
        await SyncEquiposPuntajeAsync(sesion, ct);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SyncContextoBtAsync(Sesion sesion, CancellationToken ct)
    {
        if (sesion.ContextoBT is null)
            return;

        var bt      = sesion.ContextoBT;
        var json    = MisionSnapshotPersistence.ToJson(bt.MisionSnapshot);
        var ganador = bt.GanadorEtapaActualId?.Valor;

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO contextos_bt ("SesionId", etapa_actual_index, ganador_etapa_actual_id, mision_snapshot_json)
             VALUES ({sesion.SesionId.Valor}, {bt.EtapaActualIndex}, {ganador}, {json}::jsonb)
             ON CONFLICT ("SesionId") DO UPDATE SET
                 etapa_actual_index = EXCLUDED.etapa_actual_index,
                 ganador_etapa_actual_id = EXCLUDED.ganador_etapa_actual_id,
                 mision_snapshot_json = EXCLUDED.mision_snapshot_json
             """,
            ct);
    }

    private async Task SyncContextoTriviaAsync(Sesion sesion, CancellationToken ct)
    {
        if (sesion.ContextoTrivia is null)
            return;

        var tv   = sesion.ContextoTrivia;
        var json = PreguntasOrdenadasPersistence.ToJson(tv.PreguntasOrdenadas);

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO contextos_trivia ("SesionId", pregunta_actual_index, timer_cerrado_en, preguntas_ordenadas_json)
             VALUES ({sesion.SesionId.Valor}, {tv.PreguntaActualIndex}, {tv.TimerCerradoEn}, {json}::jsonb)
             ON CONFLICT ("SesionId") DO UPDATE SET
                 pregunta_actual_index = EXCLUDED.pregunta_actual_index,
                 timer_cerrado_en = EXCLUDED.timer_cerrado_en,
                 preguntas_ordenadas_json = EXCLUDED.preguntas_ordenadas_json
             """,
            ct);
    }

    private async Task SyncEquiposPuntajeAsync(Sesion sesion, CancellationToken ct)
    {
        foreach (var equipo in sesion.Equipos)
        {
            var puntaje = equipo.PuntajeTotal;
            await _db.EquiposSesion
                .Where(e => e.EquipoId == equipo.EquipoId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(e => e.PuntajeTotal, puntaje),
                    ct);
        }
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
