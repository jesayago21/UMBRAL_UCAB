using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.SubmitEvidencia;

internal sealed class SubmitEvidenciaCommandHandler
    : IRequestHandler<SubmitEvidenciaCommand, Result<SubmitEvidenciaResult>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public SubmitEvidenciaCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _eventPublisher       = eventPublisher;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<SubmitEvidenciaResult>> Handle(
        SubmitEvidenciaCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var jugadorId = new UsuarioId(command.JugadorId);
        var participante = sesion.Participantes
                               .FirstOrDefault(p => p.JugadorId == jugadorId)
                           ?? throw new DomainException("No estás inscrito en esta sesión.");

        var evidencia = sesion.RegistrarEvidencia(
            participante.ParticipanteId,
            command.CodigoQr);

        await _sesionRepository.SaveAsync(sesion, cancellationToken);

        var eventosPista  = sesion.DomainEvents.OfType<PistaLiberada>().ToList();
        var etapaAvanzada = sesion.DomainEvents.OfType<EtapaCompletada>().Any();
        var rankingCambio = evidencia.Resultado == ResultadoValidacion.Valida;

        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        var sesionId = sesion.SesionId.Valor.ToString();

        foreach (var evt in eventosPista)
        {
            await _notificacionRealTime.NotificarPistaLiberadaAsync(
                sesionId,
                evt.ParticipanteId.Valor.ToString(),
                evt.PistaId.Valor.ToString(),
                evt.EtapaIndex,
                evt.Contenido,
                cancellationToken);
        }

        if (etapaAvanzada)
        {
            var (etapaIndex, tipoEtapa) = ResolverEtapaActual(sesion);
            await _notificacionRealTime.NotificarEtapaAvanzadaAsync(
                sesionId,
                etapaIndex,
                tipoEtapa,
                cancellationToken);
        }

        // Ranking al final: el cliente aplica el snapshot tras EtapaAvanzada/refetch.
        if (rankingCambio)
        {
            await _notificacionRealTime.NotificarRankingActualizadoAsync(
                sesionId,
                RankingNotificacionMapper.DesdeSesion(sesion),
                cancellationToken);
        }

        // Última etapa BT: Finalizar() en dominio — avisar estado a todos.
        if (sesion.Estado is EstadoSesion.Finalizada)
        {
            await _notificacionRealTime.NotificarCambioEstadoSesionAsync(
                sesionId,
                sesion.Estado.ToString(),
                cancellationToken);
        }

        return Result<SubmitEvidenciaResult>.Ok(
            new SubmitEvidenciaResult(
                evidencia.EvidenciaId.Valor,
                evidencia.Resultado.ToString()));
    }

    private static (int EtapaIndex, string TipoEtapa) ResolverEtapaActual(SesionAR sesion)
    {
        if (sesion.ContextoMision is { } ctx)
        {
            if (sesion.Estado == EstadoSesion.Finalizada)
            {
                var last = Math.Max(0, ctx.MisionSnapshot.Etapas.Count - 1);
                var tipoFinal = ctx.MisionSnapshot.Etapas.Count > 0
                    ? ctx.MisionSnapshot.Etapas[last].Tipo.ToString()
                    : TipoEtapa.BusquedaTesoro.ToString();
                return (last, tipoFinal);
            }

            var etapa = ctx.ObtenerEtapaActual();
            return (ctx.EtapaActualIndex, etapa.Tipo.ToString());
        }

        if (sesion.ContextoBT is { } bt)
            return (bt.EtapaActualIndex, TipoEtapa.BusquedaTesoro.ToString());

        return (0, TipoEtapa.BusquedaTesoro.ToString());
    }
}
