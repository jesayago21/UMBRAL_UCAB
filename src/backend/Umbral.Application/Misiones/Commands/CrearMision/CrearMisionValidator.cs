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
            .SetValidator(new EtapaMisionInputValidator());
    }

    private sealed class EtapaMisionInputValidator : AbstractValidator<EtapaMisionInput>
    {
        public EtapaMisionInputValidator()
        {
            RuleFor(x => x.TipoEtapa)
                .NotEmpty()
                .Must(t => t is "BusquedaTesoro" or "Trivia")
                .WithMessage("TipoEtapa debe ser BusquedaTesoro o Trivia.");

            When(x => string.Equals(x.TipoEtapa, "BusquedaTesoro", StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(x => x.Descripcion)
                    .NotEmpty()
                    .WithMessage("La descripción de la etapa BT es obligatoria.");

                RuleFor(x => x.CodigoQrSolucion)
                    .NotEmpty()
                    .WithMessage("El código QR solución de la etapa es obligatorio.");

                RuleForEach(x => x.Pistas!)
                    .SetValidator(new CrearPistaInputValidator())
                    .When(x => x.Pistas is not null);
            });

            When(x => string.Equals(x.TipoEtapa, "Trivia", StringComparison.OrdinalIgnoreCase), () =>
            {
                RuleFor(x => x.CategoriaIds)
                    .NotEmpty()
                    .WithMessage("La etapa trivia requiere al menos una categoría (RB-33).");
            });
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
