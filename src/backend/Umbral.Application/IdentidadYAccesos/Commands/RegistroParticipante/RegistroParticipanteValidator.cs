using FluentValidation;

namespace Umbral.Application.IdentidadYAccesos.Commands.RegistroParticipante;

public sealed class RegistroParticipanteValidator : AbstractValidator<RegistroParticipanteCommand>
{
    public RegistroParticipanteValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Username).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");
    }
}
