using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Application.Sesion;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.ProcesarCicloTriviaAutomatico;

internal sealed class ProcesarCicloTriviaAutomaticoCommandHandler
    : IRequestHandler<ProcesarCicloTriviaAutomaticoCommand, Result<int>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IPreguntaRepository _preguntaRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public ProcesarCicloTriviaAutomaticoCommandHandler(
        ISesionRepository sesionRepository,
        IPreguntaRepository preguntaRepository,
        IEventPublisher eventPublisher,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _preguntaRepository   = preguntaRepository;
        _eventPublisher       = eventPublisher;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<int>> Handle(
        ProcesarCicloTriviaAutomaticoCommand command,
        CancellationToken cancellationToken)
    {
        var ahora = command.Ahora ?? DateTimeOffset.UtcNow;
        var sesiones = await _sesionRepository.FindActivasAsync(cancellationToken);
        var acciones = 0;

        foreach (var sesion in sesiones)
        {
            var etapaAntes = sesion.ContextoMision?.EtapaActualIndex;
            var resultado = sesion.ProcesarCicloTriviaAutomatico(
                ahora,
                command.DuracionPreguntaSegundos,
                command.DuracionTransicionSegundos);

            if (resultado == ResultadoCicloTrivia.Ninguna)
                continue;

            acciones++;
            await _sesionRepository.SaveAsync(sesion, cancellationToken);

            if (sesion.DomainEvents.Count > 0)
            {
                await _eventPublisher.PublishBatchAsync(
                    sesion.DomainEvents,
                    cancellationToken);
                sesion.ClearDomainEvents();
            }

            try
            {
                await NotificarResultadoAsync(
                    sesion,
                    resultado,
                    etapaAntes,
                    cancellationToken);
            }
            catch
            {
                // Estado ya guardado: un fallo SignalR/catálogo no debe abortar el barrido.
            }
        }

        return Result<int>.Ok(acciones);
    }

    private async Task NotificarResultadoAsync(
        SesionAR sesion,
        ResultadoCicloTrivia resultado,
        int? etapaAntes,
        CancellationToken cancellationToken)
    {
        var ctx = sesion.ContextoMision!;
        var sid = sesion.SesionId.Valor.ToString();
        var cambioDeEtapa = etapaAntes is int antes && antes != ctx.EtapaActualIndex;

        switch (resultado)
        {
            case ResultadoCicloTrivia.EntroEnTransicion:
            {
                var total = ctx.ObtenerEtapaTriviaActual().PreguntasOrdenadas.Count;
                await _notificacionRealTime.NotificarTriviaEnTransicionAsync(
                    sid,
                    ctx.PreguntaTriviaActualIndex + 1,
                    total,
                    cancellationToken);
                break;
            }

            case ResultadoCicloTrivia.SecuenciaCompletada:
                await _notificacionRealTime.NotificarCambioEstadoSesionAsync(
                    sid,
                    sesion.Estado.ToString(),
                    cancellationToken);
                await _notificacionRealTime.NotificarRankingActualizadoAsync(
                    sid,
                    RankingNotificacionMapper.DesdeSesion(sesion),
                    cancellationToken);
                break;

            case ResultadoCicloTrivia.EtapaAvanzada:
            {
                var etapa = ctx.ObtenerEtapaActual();
                await _notificacionRealTime.NotificarEtapaAvanzadaAsync(
                    sid,
                    ctx.EtapaActualIndex,
                    etapa is EtapaTriviaSnapshot ? "Trivia" : "BusquedaTesoro",
                    cancellationToken);
                break;
            }

            case ResultadoCicloTrivia.SiguientePreguntaLanzada:
                if (cambioDeEtapa)
                {
                    var etapa = ctx.ObtenerEtapaActual();
                    await _notificacionRealTime.NotificarEtapaAvanzadaAsync(
                        sid,
                        ctx.EtapaActualIndex,
                        etapa is EtapaTriviaSnapshot ? "Trivia" : "BusquedaTesoro",
                        cancellationToken);
                }

                await NotificarPreguntaActualAsync(sesion, cancellationToken);
                break;
        }
    }

    private async Task NotificarPreguntaActualAsync(
        SesionAR sesion,
        CancellationToken cancellationToken)
    {
        var ctx = sesion.ContextoMision!;
        var preguntaId = ctx.ObtenerPreguntaTriviaActualId();
        var pregunta = await _preguntaRepository.FindByIdAsync(preguntaId, cancellationToken)
                       ?? throw new NotFoundException(nameof(Pregunta), preguntaId.Valor);

        var trivia = ctx.ObtenerEtapaTriviaActual();
        await _notificacionRealTime.NotificarPreguntaTriviaIniciadaAsync(
            sesion.SesionId.Valor.ToString(),
            pregunta.PreguntaId.Valor.ToString(),
            ctx.PreguntaTriviaActualIndex + 1,
            pregunta.Enunciado,
            pregunta.Opciones.Select(o => o.Texto).ToList(),
            ctx.TimerCerradoEn!.Value,
            trivia.PreguntasOrdenadas.Count,
            cancellationToken);
    }
}
