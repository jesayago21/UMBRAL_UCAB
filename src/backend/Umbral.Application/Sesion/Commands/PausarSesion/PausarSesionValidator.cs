using FluentValidation;

namespace Umbral.Application.Sesion.Commands.PausarSesion;

public sealed class PausarSesionValidator : AbstractValidator<PausarSesionCommand>
{
    public PausarSesionValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");
    }
}
