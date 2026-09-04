using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.LanzarPreguntaTrivia;

internal sealed class LanzarPreguntaTriviaCommandHandler
    : IRequestHandler<LanzarPreguntaTriviaCommand, Result<Unit>>
{
    public const int DuracionPorDefectoSegundos = 30;

    private readonly ISesionRepository _sesionRepository;
    private readonly IPreguntaRepository _preguntaRepository;
    private readonly INotificacionRealTime _notificacionRealTime;

    public LanzarPreguntaTriviaCommandHandler(
        ISesionRepository sesionRepository,
        IPreguntaRepository preguntaRepository,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _preguntaRepository   = preguntaRepository;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<Unit>> Handle(
        LanzarPreguntaTriviaCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        var duracion = command.DuracionSegundos ?? DuracionPorDefectoSegundos;
        sesion.IniciarSecuenciaTrivia(DateTimeOffset.UtcNow, duracion);
        await _sesionRepository.SaveAsync(sesion, cancellationToken);

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

        return Result<Unit>.Ok(Unit.Value);
    }
}
