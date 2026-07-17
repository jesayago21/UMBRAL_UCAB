using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Application.Sesion;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.UnirseSesion;

internal sealed class UnirseSesionCommandHandler
    : IRequestHandler<UnirseSesionCommand, Result<UnirseSesionResult>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public UnirseSesionCommandHandler(
        ISesionRepository sesionRepository,
        IEventPublisher eventPublisher,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _eventPublisher       = eventPublisher;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<UnirseSesionResult>> Handle(
        UnirseSesionCommand command,
        CancellationToken cancellationToken)
    {
        var jugadorId = new UsuarioId(command.JugadorId);

        var inscripcionExistente = await _sesionRepository.FindInscripcionAbiertaPorJugadorAsync(
            jugadorId,
            cancellationToken);

        if (inscripcionExistente is not null
            && inscripcionExistente.SesionId.Valor != command.SesionId)
        {
            throw new DomainException(
                "Ya participas en otra sesión abierta. Abandónala antes de unirte a otra.");
        }

        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var participante = sesion.UnirseParticipante(
            jugadorId,
            command.NombreParticipante,
            command.CodigoAcceso);

        await _sesionRepository.SaveAsync(sesion, cancellationToken);
        await _eventPublisher.PublishBatchAsync(
            sesion.DomainEvents,
            cancellationToken);
        sesion.ClearDomainEvents();

        var sesionId = sesion.SesionId.Valor.ToString();

        await _notificacionRealTime.NotificarParticipantesActualizadosAsync(
            sesionId,
            sesion.Participantes.Count,
            cancellationToken);

        // HU-21: empujar ranking para que los demás participantes vean el ingreso al instante.
        await _notificacionRealTime.NotificarRankingActualizadoAsync(
            sesionId,
            RankingNotificacionMapper.DesdeSesion(sesion),
            cancellationToken);

        // Si la unión abrió Programada → EnPreparacion, avisar también el estado.
        await _notificacionRealTime.NotificarCambioEstadoSesionAsync(
            sesionId,
            sesion.Estado.ToString(),
            cancellationToken);

        return Result<UnirseSesionResult>.Ok(new UnirseSesionResult(participante.ParticipanteId.Valor));
    }
}
