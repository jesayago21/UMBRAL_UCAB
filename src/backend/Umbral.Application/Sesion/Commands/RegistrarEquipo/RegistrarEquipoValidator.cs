using FluentValidation;

namespace Umbral.Application.Sesion.Commands.RegistrarEquipo;

public sealed class RegistrarEquipoValidator
    : AbstractValidator<RegistrarEquipoCommand>
{
    public RegistrarEquipoValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");

        RuleFor(x => x.NombreEquipo)
            .NotEmpty()
            .WithMessage("El nombre del equipo es obligatorio.");
    }
}
