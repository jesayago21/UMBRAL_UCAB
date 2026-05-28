using FluentValidation;

namespace Umbral.Application.Misiones.Commands.CrearMision;

public sealed class CrearMisionValidator : AbstractValidator<CrearMisionCommand>
{
    public CrearMisionValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la misión es obligatorio.");

        RuleFor(x => x.Etapas)
            .NotEmpty()
            .WithMessage("La misión debe tener al menos una etapa.");

        RuleForEach(x => x.Etapas)
            .SetValidator(new CrearEtapaInputValidator());
    }

    private sealed class CrearEtapaInputValidator : AbstractValidator<CrearEtapaInput>
    {
        public CrearEtapaInputValidator()
        {
            RuleFor(x => x.Descripcion)
                .NotEmpty()
                .WithMessage("La descripción de la etapa es obligatoria.");

            RuleFor(x => x.CodigoQrSolucion)
                .NotEmpty()
                .WithMessage("El código QR solución de la etapa es obligatorio.");

            RuleForEach(x => x.Pistas)
                .SetValidator(new CrearPistaInputValidator());
        }
    }

    private sealed class CrearPistaInputValidator : AbstractValidator<CrearPistaInput>
    {
        public CrearPistaInputValidator()
        {
            RuleFor(x => x.Contenido)
                .NotEmpty()
                .WithMessage("El contenido de la pista es obligatorio.");

            RuleFor(x => x.TipoLiberacion)
                .NotEmpty()
                .Must(x => x is "PorTiempo" or "PorGanador")
                .WithMessage("El tipo de liberación debe ser PorTiempo o PorGanador.");

            RuleFor(x => x.SegundosLiberacion)
                .GreaterThan(0)
                .When(x => x.TipoLiberacion == "PorTiempo")
                .WithMessage("PorTiempo requiere segundos de liberación mayores a cero.");
        }
    }
}
