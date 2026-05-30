using FluentValidation;

namespace Umbral.Application.Misiones.Commands.EliminarMision;

public sealed class EliminarMisionValidator : AbstractValidator<EliminarMisionCommand>
{
    public EliminarMisionValidator()
    {
        RuleFor(x => x.MisionId)
            .NotEmpty()
            .WithMessage("El identificador de la misión es obligatorio.");
    }
}
