using FluentValidation;

namespace Umbral.Application.Sesion.Commands.AplicarPenalizacion;

public sealed class AplicarPenalizacionValidator
    : AbstractValidator<AplicarPenalizacionCommand>
{
    public AplicarPenalizacionValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");

        RuleFor(x => x.ParticipanteId)
            .NotEmpty()
            .WithMessage("El identificador del participante es obligatorio.");

        RuleFor(x => x.Puntos)
            .GreaterThan(0)
            .WithMessage("Los puntos de penalización deben ser mayores a cero.");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .WithMessage("El motivo de la penalización es obligatorio.");

        RuleFor(x => x.OperadorId)
            .NotEmpty()
            .WithMessage("El identificador del operador es obligatorio.");
    }
}
