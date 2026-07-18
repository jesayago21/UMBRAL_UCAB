using FluentValidation;

namespace Umbral.Application.Misiones.Commands.AgregarPistaEtapa;

public sealed class AgregarPistaEtapaValidator : AbstractValidator<AgregarPistaEtapaCommand>
{
    public AgregarPistaEtapaValidator()
    {
        RuleFor(x => x.MisionId).NotEmpty();
        RuleFor(x => x.EtapaId).NotEmpty();
        RuleFor(x => x.Contenido).NotEmpty();
        RuleFor(x => x.TipoLiberacion)
            .NotEmpty()
            .Must(x => x is "PorTiempo" or "PorGanador" or "AlInicio")
            .WithMessage("El tipo de liberación debe ser PorTiempo o AlInicio (PorGanador).");
        RuleFor(x => x.SegundosLiberacion)
            .GreaterThan(0)
            .When(x => x.TipoLiberacion == "PorTiempo");
    }
}
