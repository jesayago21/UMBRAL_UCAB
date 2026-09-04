using FluentValidation;

namespace Umbral.Application.IdentidadYAccesos.Commands.CrearUsuario;

internal sealed class CrearUsuarioValidator : AbstractValidator<CrearUsuarioCommand>
{
    public CrearUsuarioValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Username).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Apellido).NotEmpty();
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
