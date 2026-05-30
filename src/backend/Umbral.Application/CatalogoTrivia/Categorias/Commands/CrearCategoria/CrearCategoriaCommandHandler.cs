using MediatR;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;

namespace Umbral.Application.CatalogoTrivia.Categorias.Commands.CrearCategoria;

internal sealed class CrearCategoriaCommandHandler : IRequestHandler<CrearCategoriaCommand, Result<Guid>>
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CrearCategoriaCommandHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Result<Guid>> Handle(CrearCategoriaCommand command, CancellationToken cancellationToken)
    {
        var nombre = command.Nombre.Trim();
        var exists = await _categoriaRepository.ExistsByNombreAsync(nombre, ct: cancellationToken);
        if (exists)
            throw new DomainException($"Ya existe una categoría con el nombre '{nombre}'.");

        var categoria = Categoria.Crear(command.Nombre);
        await _categoriaRepository.SaveAsync(categoria, cancellationToken);

        return Result<Guid>.Ok(categoria.CategoriaId.Valor);
    }
}
