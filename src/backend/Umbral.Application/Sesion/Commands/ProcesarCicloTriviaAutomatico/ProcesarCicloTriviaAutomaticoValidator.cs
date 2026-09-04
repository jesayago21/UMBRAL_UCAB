using FluentValidation;

namespace Umbral.Application.Sesion.Commands.ProcesarCicloTriviaAutomatico;

public sealed class ProcesarCicloTriviaAutomaticoValidator
    : AbstractValidator<ProcesarCicloTriviaAutomaticoCommand>
{
    public ProcesarCicloTriviaAutomaticoValidator()
    {
        RuleFor(x => x.DuracionPreguntaSegundos)
            .GreaterThan(0);

        RuleFor(x => x.DuracionTransicionSegundos)
            .GreaterThanOrEqualTo(0);
    }
}
