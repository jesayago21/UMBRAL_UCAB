using FluentValidation;

namespace Umbral.Application.CatalogoTrivia.Categorias.Commands.EliminarCategoria;

public sealed class EliminarCategoriaValidator : AbstractValidator<EliminarCategoriaCommand>
{
    public EliminarCategoriaValidator()
    {
        RuleFor(x => x.CategoriaId)
            .NotEmpty()
            .WithMessage("El identificador de la categoría es obligatorio.");
    }
}
