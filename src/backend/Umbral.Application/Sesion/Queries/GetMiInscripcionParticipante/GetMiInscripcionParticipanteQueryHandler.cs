using MediatR;
using Umbral.Application.Sesion.Models;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Queries.GetMiInscripcionParticipante;

internal sealed class GetMiInscripcionParticipanteQueryHandler
    : IRequestHandler<GetMiInscripcionParticipanteQuery, MiInscripcionParticipanteDto?>
{
    private readonly ISesionRepository _sesionRepository;

    public GetMiInscripcionParticipanteQueryHandler(ISesionRepository sesionRepository)
        => _sesionRepository = sesionRepository;

    public async Task<MiInscripcionParticipanteDto?> Handle(
        GetMiInscripcionParticipanteQuery query,
        CancellationToken cancellationToken)
    {
        var jugadorId = new UsuarioId(query.JugadorId);

        // Incluye Finalizada para pantalla de resultados en mobile/web.
        // Lobby filtra estados terminales y no los trata como partida en curso.
        // Cancelada no entra en vigente (evita fantasma); se limpia abajo si no hay nada.
        var sesion = await _sesionRepository.FindInscripcionVigentePorJugadorAsync(
            jugadorId,
            cancellationToken);

        if (sesion is null)
        {
            // Sin partida vigente: limpia filas huérfanas en Cancelada/Finalizada.
            await _sesionRepository.EliminarParticipacionesEnSesionesTerminalesAsync(
                jugadorId,
                cancellationToken);
            return null;
        }

        var participante = sesion.Participantes
            .FirstOrDefault(p => p.JugadorId.Valor == query.JugadorId);

        if (participante is null)
            return null;

        var titulo = sesion.Nombre;
        var ctx = sesion.ContextoMision;
        var participanteId = participante.ParticipanteId;

        return new MiInscripcionParticipanteDto(
            sesion.SesionId.Valor,
            titulo,
            participanteId.Valor,
            sesion.Estado.ToString(),
            ctx?.MisionSnapshot.Etapas.Count
                ?? sesion.ContextoBT?.MisionSnapshot.Etapas.Count
                ?? 0,
            sesion.ToEtapasParticipante(participanteId),
            ctx?.EtapaIniciadaEn,
            ctx?.SegundosPausaAcumulados ?? 0,
            ctx?.PausadaDesde,
            MapPistasPorTiempoPendientes(sesion, participanteId),
            MapPenalizaciones(sesion, participanteId),
            sesion.Participantes.Count,
            SesionAR.MaxParticipantes);
    }

    private static IReadOnlyList<PistaPorTiempoPendienteDto> MapPistasPorTiempoPendientes(
        SesionAR sesion,
        ParticipanteId participanteId)
    {
        if (sesion.ContextoMision is not { } ctx)
            return Array.Empty<PistaPorTiempoPendienteDto>();

        if (ctx.EtapaActualIndex < 0 || ctx.EtapaActualIndex >= ctx.MisionSnapshot.Etapas.Count)
            return Array.Empty<PistaPorTiempoPendienteDto>();

        if (ctx.MisionSnapshot.Etapas[ctx.EtapaActualIndex] is not EtapaBusquedaTesoroSnapshot bt)
            return Array.Empty<PistaPorTiempoPendienteDto>();

        var index = ctx.EtapaActualIndex;
        return bt.Pistas
            .Where(p =>
                p.TipoLiberacion == TipoLiberacion.PorTiempo
                && p.SegundosLiberacion is > 0
                && !ctx.YaEntregoPista(p.PistaId, index, participanteId))
            .Select(p => new PistaPorTiempoPendienteDto(p.PistaId.Valor, p.SegundosLiberacion!.Value))
            .OrderBy(p => p.SegundosLiberacion)
            .ToList();
    }

    private static IReadOnlyList<PenalizacionParticipanteDto> MapPenalizaciones(
        SesionAR sesion,
        ParticipanteId participanteId)
    {
        var idStr = participanteId.Valor.ToString();
        return sesion.HistorialEventos
            .Where(e => e.Tipo == "PenalizacionAplicada")
            .Select(e => TryParse(e, idStr))
            .Where(p => p is not null)
            .Select(p => p!)
            .OrderByDescending(p => p.OcurridoEn)
            .ToList();
    }

    private static PenalizacionParticipanteDto? TryParse(EventoSesion evento, string participanteId)
    {
        // Payload: participante={guid};puntos={n};motivo={texto libre}
        var payload = evento.Payload;
        const string prefijoPart = "participante=";
        const string prefijoPuntos = ";puntos=";
        const string prefijoMotivo = ";motivo=";

        var iPart = payload.IndexOf(prefijoPart, StringComparison.Ordinal);
        var iPuntos = payload.IndexOf(prefijoPuntos, StringComparison.Ordinal);
        var iMotivo = payload.IndexOf(prefijoMotivo, StringComparison.Ordinal);
        if (iPart < 0 || iPuntos < 0 || iMotivo < 0)
            return null;

        var idRaw = payload.Substring(
            iPart + prefijoPart.Length,
            iPuntos - (iPart + prefijoPart.Length));
        if (!string.Equals(idRaw, participanteId, StringComparison.OrdinalIgnoreCase))
            return null;

        var puntosRaw = payload.Substring(
            iPuntos + prefijoPuntos.Length,
            iMotivo - (iPuntos + prefijoPuntos.Length));
        if (!int.TryParse(puntosRaw, out var puntos))
            return null;

        var motivo = payload[(iMotivo + prefijoMotivo.Length)..].Trim();
        return new PenalizacionParticipanteDto(puntos, motivo, evento.OcurridoEn);
    }
}
