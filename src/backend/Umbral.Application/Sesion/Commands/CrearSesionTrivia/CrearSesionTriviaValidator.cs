using FluentValidation;

namespace Umbral.Application.Sesion.Commands.CrearSesionTrivia;

public sealed class CrearSesionTriviaValidator
    : AbstractValidator<CrearSesionTriviaCommand>
{
    public CrearSesionTriviaValidator()
    {
        RuleFor(x => x.CategoriaIds)
            .NotEmpty()
            .WithMessage("Selecciona al menos una categoría para la sesión de trivia.");

        RuleFor(x => x.OperadorId)
            .NotEmpty()
            .WithMessage("El identificador del operador es obligatorio.");
    }
}
