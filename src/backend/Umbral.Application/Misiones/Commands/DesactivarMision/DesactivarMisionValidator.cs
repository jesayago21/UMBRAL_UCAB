using FluentValidation;

namespace Umbral.Application.Misiones.Commands.DesactivarMision;

public sealed class DesactivarMisionValidator : AbstractValidator<DesactivarMisionCommand>
{
    public DesactivarMisionValidator()
    {
        RuleFor(x => x.MisionId)
            .NotEmpty()
            .WithMessage("El identificador de la misión es obligatorio.");
    }
}
