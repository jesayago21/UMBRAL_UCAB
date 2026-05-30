using FluentValidation;

namespace Umbral.Application.CatalogoTrivia.Categorias.Commands.ActualizarCategoria;

public sealed class ActualizarCategoriaValidator : AbstractValidator<ActualizarCategoriaCommand>
{
    public ActualizarCategoriaValidator()
    {
        RuleFor(x => x.CategoriaId)
            .NotEmpty()
            .WithMessage("El identificador de la categoría es obligatorio.");

        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la categoría es obligatorio.");
    }
}
