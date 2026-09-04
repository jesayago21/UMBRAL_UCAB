using FluentValidation;

namespace Umbral.Application.Sesion.Commands.SubmitRespuestaTrivia;

public sealed class SubmitRespuestaTriviaValidator
    : AbstractValidator<SubmitRespuestaTriviaCommand>
{
    public SubmitRespuestaTriviaValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");

        RuleFor(x => x.JugadorId)
            .NotEmpty()
            .WithMessage("El identificador del jugador es obligatorio.");

        RuleFor(x => x.PreguntaId)
            .NotEmpty()
            .WithMessage("El identificador de la pregunta es obligatorio.");

        RuleFor(x => x.IndiceOpcion)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El índice de opción no puede ser negativo.");

        RuleFor(x => x.DuracionTimerSegundos)
            .GreaterThan(0)
            .When(x => x.DuracionTimerSegundos.HasValue)
            .WithMessage("La duración del timer debe ser mayor a cero.");
    }
}
