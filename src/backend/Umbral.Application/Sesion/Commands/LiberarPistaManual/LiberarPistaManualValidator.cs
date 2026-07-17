using FluentValidation;

namespace Umbral.Application.Sesion.Commands.LiberarPistaManual;

public sealed class LiberarPistaManualValidator
    : AbstractValidator<LiberarPistaManualCommand>
{
    public LiberarPistaManualValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");

        RuleFor(x => x.Contenido)
            .NotEmpty()
            .WithMessage("El contenido de la pista es obligatorio.")
            .MaximumLength(2000)
            .WithMessage("El contenido de la pista no puede superar 2000 caracteres.");
    }
}
