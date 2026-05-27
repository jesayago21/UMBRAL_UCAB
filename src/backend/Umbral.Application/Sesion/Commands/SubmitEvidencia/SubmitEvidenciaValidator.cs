using FluentValidation;

namespace Umbral.Application.Sesion.Commands.SubmitEvidencia;

public sealed class SubmitEvidenciaValidator
    : AbstractValidator<SubmitEvidenciaCommand>
{
    public SubmitEvidenciaValidator()
    {
        RuleFor(x => x.SesionId)
            .NotEmpty()
            .WithMessage("El identificador de la sesión es obligatorio.");

        RuleFor(x => x.EquipoId)
            .NotEmpty()
            .WithMessage("El identificador del equipo es obligatorio.");

        RuleFor(x => x.CodigoQr)
            .NotEmpty()
            .WithMessage("El código QR es obligatorio.");
    }
}
