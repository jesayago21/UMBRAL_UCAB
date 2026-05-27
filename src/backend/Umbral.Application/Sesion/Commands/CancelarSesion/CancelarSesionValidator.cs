using FluentValidation;

namespace Umbral.Application.Sesion.Commands.CancelarSesion;

public sealed class CancelarSesionValidator : AbstractValidator<CancelarSesionCommand>
{
    public CancelarSesionValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .WithMessage("El motivo de cancelación es obligatorio.");
    }
}
