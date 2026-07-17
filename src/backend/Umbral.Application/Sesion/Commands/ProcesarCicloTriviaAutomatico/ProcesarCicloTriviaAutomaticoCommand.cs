using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.Sesion.Commands.ProcesarCicloTriviaAutomatico;

/// <summary>
/// HU-33 / HU-38 — tick del motor: cierra timers vencidos y lanza la siguiente pregunta.
/// </summary>
public sealed record ProcesarCicloTriviaAutomaticoCommand(
    DateTimeOffset? Ahora = null,
    int DuracionPreguntaSegundos = 30,
    int DuracionTransicionSegundos = 5)
    : IRequest<Result<int>>;
