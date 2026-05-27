using FluentValidation;

namespace Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;

public sealed class CrearSesionBusquedaTesoroValidator
    : AbstractValidator<CrearSesionBusquedaTesoroCommand>
{
    public CrearSesionBusquedaTesoroValidator()
    {
        RuleFor(x => x.MisionId)
            .NotEmpty()
            .WithMessage("El identificador de la misión es obligatorio.");

        RuleFor(x => x.OperadorId)
            .NotEmpty()
            .WithMessage("El identificador del operador es obligatorio.");
    }
}
