using FluentValidation;

namespace Umbral.Application.Sesion.Commands.UnirseSesion;

public sealed class UnirseSesionValidator : AbstractValidator<UnirseSesionCommand>
{
    public UnirseSesionValidator()
    {
        RuleFor(x => x.SesionId).NotEmpty();
        RuleFor(x => x.CodigoAcceso).NotEmpty();
        RuleFor(x => x.JugadorId).NotEmpty();
        RuleFor(x => x.NombreParticipante).NotEmpty();
    }
}
