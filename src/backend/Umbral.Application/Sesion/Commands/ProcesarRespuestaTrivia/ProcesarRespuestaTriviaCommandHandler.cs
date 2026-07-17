using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.ProcesarRespuestaTrivia;

internal sealed class ProcesarRespuestaTriviaCommandHandler
    : IRequestHandler<ProcesarRespuestaTriviaCommand, Result<ProcesarRespuestaTriviaResult>>
{
    public const int DuracionPorDefectoSegundos = 30;

    private readonly ISesionRepository _sesionRepository;
    private readonly IPreguntaRepository _preguntaRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly INotificacionRealTime _notificacionRealTime;

    public ProcesarRespuestaTriviaCommandHandler(
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

    public async Task<Result<ProcesarRespuestaTriviaResult>> Handle(
        ProcesarRespuestaTriviaCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var jugadorId = new UsuarioId(command.JugadorId);
        var participante = sesion.Participantes.FirstOrDefault(p => p.JugadorId == jugadorId)
                           ?? throw new DomainException("No estás inscrito en esta sesión.");

        var pregunta = await _preguntaRepository.FindByIdAsync(
                           new PreguntaId(command.PreguntaId),
                           cancellationToken)
                       ?? throw new NotFoundException(nameof(Pregunta), command.PreguntaId);

        var respuesta = sesion.RegistrarRespuestaTrivia(
            participante.ParticipanteId,
            pregunta,
            command.IndiceOpcion,
            command.RecibidoEnUtc,
            command.DuracionTimerSegundos);

        await _sesionRepository.SaveAsync(sesion, cancellationToken);

        await _eventPublisher.PublishBatchAsync(sesion.DomainEvents, cancellationToken);
        sesion.ClearDomainEvents();

        await _notificacionRealTime.NotificarRankingActualizadoAsync(
            sesion.SesionId.Valor.ToString(),
            RankingNotificacionMapper.DesdeSesion(sesion),
            cancellationToken);

        return Result<ProcesarRespuestaTriviaResult>.Ok(
            new ProcesarRespuestaTriviaResult(
                respuesta.RespuestaTriviaId.Valor,
                respuesta.EsCorrecta,
                respuesta.FueraDeTiempo,
                respuesta.PuntosOtorgados,
                participante.PuntajeTotal.Valor));
    }
}
