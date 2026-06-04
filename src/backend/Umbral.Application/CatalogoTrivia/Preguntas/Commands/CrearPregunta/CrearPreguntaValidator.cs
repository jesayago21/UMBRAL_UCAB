using FluentValidation;
using Umbral.Application.CatalogoTrivia.Models;

namespace Umbral.Application.CatalogoTrivia.Preguntas.Commands.CrearPregunta;

public sealed class CrearPreguntaValidator : AbstractValidator<CrearPreguntaCommand>
{
    public CrearPreguntaValidator()
    {
        RuleFor(x => x.Enunciado)
            .NotEmpty()
            .WithMessage("El enunciado de la pregunta es obligatorio.");

        RuleFor(x => x.Dificultad)
            .NotEmpty()
            .Must(x => x is "Facil" or "Media" or "Dificil")
            .WithMessage("La dificultad debe ser Facil, Media o Dificil.");

        RuleFor(x => x.Opciones)
            .NotEmpty()
            .Must(o => o.Count >= 3)
            .WithMessage("La pregunta debe tener al menos 3 opciones de respuesta.");

        RuleFor(x => x.Opciones)
            .Must(o => o.Count(x => x.EsCorrecta) == 1)
            .WithMessage("La pregunta debe tener exactamente una opción correcta.");

        RuleForEach(x => x.Opciones)
            .SetValidator(new OpcionRespuestaInputValidator());
    }

    private sealed class OpcionRespuestaInputValidator : AbstractValidator<OpcionRespuestaInput>
    {
        public OpcionRespuestaInputValidator()
        {
            RuleFor(x => x.Texto)
                .NotEmpty()
                .WithMessage("El texto de la opción es obligatorio.");
        }
    }
}
