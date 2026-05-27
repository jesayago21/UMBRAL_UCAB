using FluentValidation;

namespace Umbral.Application.Sesion.Commands.ReanudarSesion;

public sealed class ReanudarSesionValidator : AbstractValidator<ReanudarSesionCommand>
{
    public ReanudarSesionValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");
    }
}
