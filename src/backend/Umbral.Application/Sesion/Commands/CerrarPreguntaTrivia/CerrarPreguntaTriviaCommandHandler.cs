using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Sesion.Commands.CerrarPreguntaTrivia;

internal sealed class CerrarPreguntaTriviaCommandHandler
    : IRequestHandler<CerrarPreguntaTriviaCommand, Result<Unit>>
{
    private readonly ISesionRepository _sesionRepository;
    private readonly INotificacionRealTime _notificacionRealTime;

    public CerrarPreguntaTriviaCommandHandler(
        ISesionRepository sesionRepository,
        INotificacionRealTime notificacionRealTime)
    {
        _sesionRepository     = sesionRepository;
        _notificacionRealTime = notificacionRealTime;
    }

    public async Task<Result<Unit>> Handle(
        CerrarPreguntaTriviaCommand command,
        CancellationToken cancellationToken)
    {
        var sesion = await _sesionRepository.FindByIdAsync(
                         new SesionId(command.SesionId),
                         cancellationToken)
                     ?? throw new NotFoundException(nameof(SesionAR), command.SesionId);

        sesion.CerrarPreguntaTriviaYEntrarTransicion();
        await _sesionRepository.SaveAsync(sesion, cancellationToken);

        var ctx = sesion.ContextoMision!;
        var total = ctx.ObtenerEtapaTriviaActual().PreguntasOrdenadas.Count;

        await _notificacionRealTime.NotificarTriviaEnTransicionAsync(
            sesion.SesionId.Valor.ToString(),
            ctx.PreguntaTriviaActualIndex + 1,
            total,
            cancellationToken);

        return Result<Unit>.Ok(Unit.Value);
    }
}
