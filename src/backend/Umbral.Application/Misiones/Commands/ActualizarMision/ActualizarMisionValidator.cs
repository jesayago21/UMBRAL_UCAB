using FluentValidation;

namespace Umbral.Application.Misiones.Commands.ActualizarMision;

public sealed class ActualizarMisionValidator : AbstractValidator<ActualizarMisionCommand>
{
    public ActualizarMisionValidator()
    {
        RuleFor(x => x.MisionId)
            .NotEmpty()
            .WithMessage("El identificador de la misión es obligatorio.");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la misión es obligatorio.");
    }
}
