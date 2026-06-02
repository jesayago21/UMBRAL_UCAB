using FluentValidation;

namespace Umbral.Application.IdentidadYAccesos.Commands.ActualizarUsuario;

public sealed class ActualizarUsuarioValidator : AbstractValidator<ActualizarUsuarioCommand>
{
    public ActualizarUsuarioValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Apellido).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Rol).NotEmpty();
        RuleFor(x => x.NuevaPassword)
            .MinimumLength(8)
            .When(x => !string.IsNullOrWhiteSpace(x.NuevaPassword));
    }
}
