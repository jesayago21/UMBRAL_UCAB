using FluentValidation;

namespace Umbral.Application.IdentidadYAccesos.Commands.ActualizarUsuario;

public sealed class ActualizarUsuarioValidator : AbstractValidator<ActualizarUsuarioCommand>
{
    public ActualizarUsuarioValidator()
    {
        RuleFor(x => x.KeycloakUserId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Rol).NotEmpty();
        RuleFor(x => x.Rol)
            .Must(r => !string.Equals(r, "Participante", StringComparison.OrdinalIgnoreCase))
            .WithMessage("El rol Participante no se asigna desde la administración de usuarios (RB-35).");
        RuleFor(x => x.NuevaPassword)
            .MinimumLength(8)
            .When(x => !string.IsNullOrWhiteSpace(x.NuevaPassword));
    }
}
