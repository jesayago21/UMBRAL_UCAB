using FluentValidation;

namespace Umbral.Application.IdentidadYAccesos.Commands.AsignarRolesUsuario;

internal sealed class AsignarRolesUsuarioValidator : AbstractValidator<AsignarRolesUsuarioCommand>
{
    public AsignarRolesUsuarioValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
        RuleFor(x => x.Roles)
            .Must(r => r.Count == 1)
            .WithMessage("Debe indicar exactamente un rol.");
    }
}
