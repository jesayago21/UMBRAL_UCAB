using FluentValidation;

namespace Umbral.Application.Sesion.Commands.CerrarPreguntaTrivia;

public sealed class CerrarPreguntaTriviaValidator
    : AbstractValidator<CerrarPreguntaTriviaCommand>
{
    public CerrarPreguntaTriviaValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");
    }
}
