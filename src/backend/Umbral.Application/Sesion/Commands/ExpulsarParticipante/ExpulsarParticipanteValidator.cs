using FluentValidation;

namespace Umbral.Application.Sesion.Commands.ExpulsarParticipante;

public sealed class ExpulsarParticipanteValidator
    : AbstractValidator<ExpulsarParticipanteCommand>
{
    public ExpulsarParticipanteValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");

        RuleFor(x => x.ParticipanteId)
            .NotEmpty()
            .WithMessage("El identificador del participante es obligatorio.");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .WithMessage("El motivo de la expulsión es obligatorio.");
    }
}
