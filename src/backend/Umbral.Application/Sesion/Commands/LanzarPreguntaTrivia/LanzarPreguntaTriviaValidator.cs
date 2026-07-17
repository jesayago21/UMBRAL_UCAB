using FluentValidation;

namespace Umbral.Application.Sesion.Commands.LanzarPreguntaTrivia;

public sealed class LanzarPreguntaTriviaValidator
    : AbstractValidator<LanzarPreguntaTriviaCommand>
{
    public LanzarPreguntaTriviaValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");

        RuleFor(x => x.DuracionSegundos)
            .GreaterThan(0)
            .When(x => x.DuracionSegundos.HasValue)
            .WithMessage("La duración del timer debe ser mayor a cero.");
    }
}
