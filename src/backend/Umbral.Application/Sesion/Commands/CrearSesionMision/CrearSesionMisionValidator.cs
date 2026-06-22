using FluentValidation;

namespace Umbral.Application.Sesion.Commands.CrearSesionMision;

public sealed class CrearSesionMisionValidator : AbstractValidator<CrearSesionMisionCommand>
{
    public CrearSesionMisionValidator()
    {
        RuleFor(x => x.MisionId).NotEmpty();
        RuleFor(x => x.OperadorId).NotEmpty();
        RuleFor(x => x.NombreSesion).NotEmpty().MaximumLength(120);
    }
}
