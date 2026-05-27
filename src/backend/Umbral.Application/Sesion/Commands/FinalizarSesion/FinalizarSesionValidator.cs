using FluentValidation;

namespace Umbral.Application.Sesion.Commands.FinalizarSesion;

public sealed class FinalizarSesionValidator : AbstractValidator<FinalizarSesionCommand>
{
    public FinalizarSesionValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");
    }
}
