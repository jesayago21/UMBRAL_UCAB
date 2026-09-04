using FluentValidation;

namespace Umbral.Application.Sesion.Commands.ProcesarRespuestaTrivia;

public sealed class ProcesarRespuestaTriviaValidator
    : AbstractValidator<ProcesarRespuestaTriviaCommand>
{
    public ProcesarRespuestaTriviaValidator()
    {
        RuleFor(x => x.SesionId).NotEmpty();
        RuleFor(x => x.JugadorId).NotEmpty();
        RuleFor(x => x.PreguntaId).NotEmpty();
        RuleFor(x => x.IndiceOpcion).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DuracionTimerSegundos).GreaterThan(0);
    }
}
