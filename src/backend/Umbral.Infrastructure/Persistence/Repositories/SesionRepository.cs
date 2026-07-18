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
            .Include(s => s.ContextoMision)
            .Include(s => s.ContextoBT)
            .Include(s => s.ContextoTrivia)
            .Include("_participantes")
            .Include("_historialEventos")
            .Include("_evidencias")
            .Include("_respuestasTrivia")
            .FirstOrDefaultAsync(x => x.SesionId == id, ct);
    }

    public async Task<IReadOnlyList<Sesion>> FindActivasAsync(CancellationToken ct = default)
    {
        // Background services poll this often — no historial/evidencias (crecen sin límite).
        return await _db.Sesiones
            .AsNoTracking()
            .Include(s => s.ContextoMision)
            .Include("_participantes")
            .Where(x => x.Estado == EstadoSesion.Activa)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Sesion>> FindOperativasByOperadorAsync(
        UsuarioId operadorId,
        CancellationToken ct = default)
    {
        return await _db.Sesiones
            .AsNoTracking()
            .Include(s => s.ContextoMision)
            .Include(s => s.ContextoBT)
            .Include(s => s.ContextoTrivia)
            .Include("_participantes")
            .Where(x =>
                x.OperadorId == operadorId &&
                x.Estado != EstadoSesion.Finalizada &&
                x.Estado != EstadoSesion.Cancelada)
            .OrderByDescending(x => x.IniciadaEn)
            .ThenByDescending(x => x.SesionId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Sesion>> FindDisponiblesParaParticipanteAsync(
        TipoSesion? tipo,
        CancellationToken ct = default)
    {
        var query = _db.Sesiones
            .AsNoTracking()
            .Include(s => s.ContextoMision)
            .Include(s => s.ContextoBT)
            .Include(s => s.ContextoTrivia)
            .Include("_participantes")
            .Where(x => x.Estado == EstadoSesion.EnPreparacion);

        if (tipo.HasValue)
            query = query.Where(x => x.TipoSesion == tipo.Value);

        return await query
            .OrderByDescending(x => x.SesionId)
            .ToListAsync(ct);
    }

    public async Task<Sesion?> FindInscripcionAbiertaPorJugadorAsync(
        UsuarioId jugadorId,
        CancellationToken ct = default)
    {
        var sesionId = await (
                from p in _db.ParticipantesSesion.AsNoTracking()
                join s in _db.Sesiones.AsNoTracking() on p.SesionId equals s.SesionId
                where p.JugadorId == jugadorId
                      && s.Estado != EstadoSesion.Finalizada
                      && s.Estado != EstadoSesion.Cancelada
                orderby s.SesionId descending
                select s.SesionId)
            .FirstOrDefaultAsync(ct);

        if (sesionId is null)
            return null;

        return await FindByIdAsync(sesionId, ct);
    }

    public async Task<Sesion?> FindInscripcionVigentePorJugadorAsync(
        UsuarioId jugadorId,
        CancellationToken ct = default)
    {
        var candidatos = await (
                from p in _db.ParticipantesSesion.AsNoTracking()
                join s in _db.Sesiones.AsNoTracking() on p.SesionId equals s.SesionId
                where p.JugadorId == jugadorId
                      // Cancelada no es “mi partida”: evitaba lobby fantasma en mobile/web.
                      && s.Estado != EstadoSesion.Cancelada
                select new
                {
                    SesionId     = s.SesionId,
                    s.Estado,
                    s.IniciadaEn,
                    s.FinalizadaEn
                })
            .ToListAsync(ct);

        if (candidatos.Count == 0)
            return null;

        static int Prioridad(EstadoSesion estado) => estado switch
        {
            EstadoSesion.Activa or EstadoSesion.Pausada => 0,
            EstadoSesion.EnPreparacion or EstadoSesion.Programada => 1,
            EstadoSesion.Finalizada or EstadoSesion.Cancelada => 2,
            _ => 3
        };

        var elegido = candidatos
            .OrderBy(c => Prioridad(c.Estado))
            .ThenByDescending(c => c.FinalizadaEn ?? c.IniciadaEn)
            .ThenByDescending(c => c.SesionId.Valor)
            .First();

        return await FindByIdAsync(elegido.SesionId, ct);
    }

    public async Task<int> EliminarParticipacionesEnSesionesTerminalesAsync(
        UsuarioId jugadorId,
        CancellationToken ct = default)
    {
        return await _db.ParticipantesSesion
            .Where(p => p.JugadorId == jugadorId)
            .Where(p => _db.Sesiones.Any(s =>
                s.SesionId == p.SesionId
                && (s.Estado == EstadoSesion.Finalizada || s.Estado == EstadoSesion.Cancelada)))
            .ExecuteDeleteAsync(ct);
    }

    public Task<bool> ExisteNombreSesionOperativaAsync(string nombre, CancellationToken ct = default)
    {
        var normalized = nombre.Trim().ToLowerInvariant();
        return _db.Sesiones.AnyAsync(
            x => x.Nombre.ToLower() == normalized
                 && x.Estado != EstadoSesion.Finalizada
                 && x.Estado != EstadoSesion.Cancelada,
            ct);
    }

    public async Task<int> CountEventosHistorialAsync(SesionId sesionId, CancellationToken ct = default)
    {
        return await _db.EventosSesion
            .AsNoTracking()
            .CountAsync(x => x.SesionId == sesionId, ct);
    }

    public async Task<IReadOnlyList<EventoSesion>> ListEventosHistorialAsync(
        SesionId sesionId,
        int pagina,
        int tamanoPagina,
        CancellationToken ct = default)
    {
        var skip = Math.Max(0, (pagina - 1) * tamanoPagina);
        return await _db.EventosSesion
            .AsNoTracking()
            .Where(x => x.SesionId == sesionId)
            .OrderByDescending(x => x.OcurridoEn)
            .Skip(skip)
            .Take(tamanoPagina)
            .ToListAsync(ct);
    }

    public async Task EliminarParticipanteAsync(ParticipanteId participanteId, CancellationToken ct = default)
    {
        await _db.ParticipantesSesion
            .Where(e => e.ParticipanteId == participanteId)
            .ExecuteDeleteAsync(ct);
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

        await SyncContextoMisionAsync(sesion, ct);
        await SyncContextoBtAsync(sesion, ct);
        await SyncContextoTriviaAsync(sesion, ct);
        await InsertNewChildrenAsync(sesion, ct);
        await SyncParticipantesPuntajeAsync(sesion, ct);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SyncContextoMisionAsync(Sesion sesion, CancellationToken ct)
    {
        if (sesion.ContextoMision is null)
            return;

        var cm      = sesion.ContextoMision;
        var json    = MisionSnapshotPersistence.ToJson(cm.MisionSnapshot);
        var ganador = cm.GanadorEtapaActualId?.Valor;
        var pistasJson = PistasEntregadasPersistence.ToJson(cm.PistasEntregadas);
        var etapaIniciada = cm.EtapaIniciadaEn;
        var pausadaDesde = cm.PausadaDesde;
        var segundosPausa = cm.SegundosPausaAcumulados;
        var timerCerrado = cm.TimerCerradoEn;
        var triviaTransicion = cm.TriviaEnTransicion;
        var transicionHasta = cm.TransicionHasta;

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO contextos_mision (
                 "SesionId", mision_id, etapa_actual_index, ganador_etapa_actual_id,
                 pregunta_trivia_actual_index, mision_snapshot_json,
                 etapa_iniciada_en, pausada_desde, segundos_pausa_acumulados, pistas_entregadas_json,
                 timer_cerrado_en, trivia_en_transicion, transicion_hasta)
             VALUES (
                 {sesion.SesionId.Valor}, {cm.MisionId.Valor}, {cm.EtapaActualIndex}, {ganador},
                 {cm.PreguntaTriviaActualIndex}, {json}::jsonb,
                 {etapaIniciada}, {pausadaDesde}, {segundosPausa}, {pistasJson}::jsonb,
                 {timerCerrado}, {triviaTransicion}, {transicionHasta})
             ON CONFLICT ("SesionId") DO UPDATE SET
                 etapa_actual_index = EXCLUDED.etapa_actual_index,
                 ganador_etapa_actual_id = EXCLUDED.ganador_etapa_actual_id,
                 pregunta_trivia_actual_index = EXCLUDED.pregunta_trivia_actual_index,
                 mision_snapshot_json = EXCLUDED.mision_snapshot_json,
                 etapa_iniciada_en = EXCLUDED.etapa_iniciada_en,
                 pausada_desde = EXCLUDED.pausada_desde,
                 segundos_pausa_acumulados = EXCLUDED.segundos_pausa_acumulados,
                 pistas_entregadas_json = EXCLUDED.pistas_entregadas_json,
                 timer_cerrado_en = EXCLUDED.timer_cerrado_en,
                 trivia_en_transicion = EXCLUDED.trivia_en_transicion,
                 transicion_hasta = EXCLUDED.transicion_hasta
             """,
            ct);
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
             INSERT INTO contextos_trivia ("SesionId", pregunta_actual_index, timer_cerrado_en, categorias_titulo, preguntas_ordenadas_json)
             VALUES ({sesion.SesionId.Valor}, {tv.PreguntaActualIndex}, {tv.TimerCerradoEn}, {tv.CategoriasTitulo}, {json}::jsonb)
             ON CONFLICT ("SesionId") DO UPDATE SET
                 pregunta_actual_index = EXCLUDED.pregunta_actual_index,
                 timer_cerrado_en = EXCLUDED.timer_cerrado_en,
                 categorias_titulo = EXCLUDED.categorias_titulo,
                 preguntas_ordenadas_json = EXCLUDED.preguntas_ordenadas_json
             """,
            ct);
    }

    private async Task SyncParticipantesPuntajeAsync(Sesion sesion, CancellationToken ct)
    {
        foreach (var participante in sesion.Participantes)
        {
            // Valor CLR int: ExecuteUpdate + value converter a veces no aplica el VO completo.
            var puntajeValor = participante.PuntajeTotal.Valor;
            var deudaValor = participante.DeudaPendiente.Valor;
            await _db.ParticipantesSesion
                .Where(e => e.ParticipanteId == participante.ParticipanteId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(e => e.PuntajeTotal, Puntaje.Crear(puntajeValor))
                        .SetProperty(e => e.DeudaPendiente, Puntaje.Crear(deudaValor)),
                    ct);
        }
    }

    private async Task InsertNewChildrenAsync(Sesion sesion, CancellationToken ct)
    {
        foreach (var participante in sesion.Participantes)
        {
            var exists = await _db.ParticipantesSesion
                .AnyAsync(e => e.ParticipanteId == participante.ParticipanteId, ct);

            if (!exists)
                await _db.ParticipantesSesion.AddAsync(participante, ct);
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

        foreach (var respuesta in sesion.RespuestasTrivia)
        {
            var exists = await _db.RespuestasTrivia
                .AnyAsync(r => r.RespuestaTriviaId == respuesta.RespuestaTriviaId, ct);

            if (!exists)
                await _db.RespuestasTrivia.AddAsync(respuesta, ct);
        }
    }
}
