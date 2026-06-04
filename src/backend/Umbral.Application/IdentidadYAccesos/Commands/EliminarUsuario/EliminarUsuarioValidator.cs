using FluentValidation;

namespace Umbral.Application.IdentidadYAccesos.Commands.EliminarUsuario;

public sealed class EliminarUsuarioValidator : AbstractValidator<EliminarUsuarioCommand>
{
    public EliminarUsuarioValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty();
    }
}
