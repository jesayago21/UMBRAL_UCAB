using FluentValidation;

namespace Umbral.Application.IdentidadYAccesos.Commands.AsignarRolesUsuario;

internal sealed class AsignarRolesUsuarioValidator : AbstractValidator<AsignarRolesUsuarioCommand>
{
    public AsignarRolesUsuarioValidator()
    {
        RuleFor(x => x.KeycloakUserId).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
        RuleFor(x => x.Roles)
            .Must(r => r.Count == 1)
            .WithMessage("Debe indicar exactamente un rol.");
        RuleFor(x => x.Roles)
            .Must(r => !r.Any(role =>
                string.Equals(role, "Participante", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("El rol Participante no se asigna desde la administración de usuarios (RB-35).");
    }
}
