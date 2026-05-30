using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;

namespace Umbral.Application.CatalogoTrivia.Categorias.Commands.ActualizarCategoria;

internal sealed class ActualizarCategoriaCommandHandler
    : IRequestHandler<ActualizarCategoriaCommand, Result<Guid>>
{
    private readonly ICategoriaRepository _categoriaRepository;

    public ActualizarCategoriaCommandHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<Result<Guid>> Handle(
        ActualizarCategoriaCommand command,
        CancellationToken cancellationToken)
    {
        var categoria = await _categoriaRepository.FindByIdAsync(
            new CategoriaId(command.CategoriaId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), command.CategoriaId);

        if (categoria.Eliminada)
            throw new NotFoundException(nameof(Categoria), command.CategoriaId);

        var nombre = command.Nombre.Trim();
        var exists = await _categoriaRepository.ExistsByNombreAsync(
            nombre,
            command.CategoriaId,
            cancellationToken);
        if (exists)
            throw new DomainException($"Ya existe una categoría con el nombre '{nombre}'.");

        categoria.Renombrar(command.Nombre);
        await _categoriaRepository.SaveAsync(categoria, cancellationToken);

        return Result<Guid>.Ok(categoria.CategoriaId.Valor);
    }
}
