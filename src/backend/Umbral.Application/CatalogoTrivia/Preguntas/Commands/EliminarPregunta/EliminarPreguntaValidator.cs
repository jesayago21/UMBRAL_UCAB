using FluentValidation;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Commands.EliminarPregunta;

public sealed class EliminarPreguntaValidator : AbstractValidator<EliminarPreguntaCommand>
{
    public EliminarPreguntaValidator()
    {
        RuleFor(x => x.PreguntaId)
            .NotEmpty()
            .WithMessage("El identificador de la pregunta es obligatorio.");
    }
}
