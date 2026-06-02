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
        RuleFor(x => x.PasswordTemporal).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Roles).NotEmpty();
        RuleFor(x => x.Roles)
            .Must(r => r.Count == 1)
            .WithMessage("Debe indicar exactamente un rol.");
    }
}
