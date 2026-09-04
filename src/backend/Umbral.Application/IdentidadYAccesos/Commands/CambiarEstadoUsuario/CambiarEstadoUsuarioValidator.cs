using FluentValidation;

namespace Umbral.Application.IdentidadYAccesos.Commands.CambiarEstadoUsuario;

internal sealed class CambiarEstadoUsuarioValidator : AbstractValidator<CambiarEstadoUsuarioCommand>
{
    public CambiarEstadoUsuarioValidator()
    {
        RuleFor(x => x.KeycloakUserId).NotEmpty();
        RuleFor(x => x.Accion).NotEmpty();
        RuleFor(x => x.Accion)
            .Must(a =>
                string.Equals(a, "Activar", StringComparison.OrdinalIgnoreCase)
                || string.Equals(a, "Bloquear", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Acción inválida. Use Activar o Bloquear.");
    }
}
