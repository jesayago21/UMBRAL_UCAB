using FluentValidation;

namespace Umbral.Application.Sesion.Commands.CrearSesionMision;

internal sealed class CrearSesionMisionValidator : AbstractValidator<CrearSesionMisionCommand>
{
    public CrearSesionMisionValidator()
    {
        RuleFor(x => x.MisionId).NotEmpty();
        RuleFor(x => x.OperadorId).NotEmpty();
    }
}
