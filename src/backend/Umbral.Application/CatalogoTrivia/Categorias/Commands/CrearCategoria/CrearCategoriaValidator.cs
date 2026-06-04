using FluentValidation;

namespace Umbral.Application.CatalogoTrivia.Categorias.Commands.CrearCategoria;

public sealed class CrearCategoriaValidator : AbstractValidator<CrearCategoriaCommand>
{
    public CrearCategoriaValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithMessage("El nombre de la categoría es obligatorio.");
    }
}
