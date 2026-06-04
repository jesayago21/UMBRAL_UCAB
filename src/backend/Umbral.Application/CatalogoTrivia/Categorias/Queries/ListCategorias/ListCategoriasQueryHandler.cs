using MediatR;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Domain.CatalogoTrivia.Categoria;

namespace Umbral.Application.CatalogoTrivia.Categorias.Queries.ListCategorias;

internal sealed class ListCategoriasQueryHandler
    : IRequestHandler<ListCategoriasQuery, IReadOnlyList<CategoriaDto>>
{
    private readonly ICategoriaRepository _categoriaRepository;

    public ListCategoriasQueryHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IReadOnlyList<CategoriaDto>> Handle(
        ListCategoriasQuery query,
        CancellationToken cancellationToken)
    {
        var categorias = await _categoriaRepository.FindAllAsync(cancellationToken);

        var activas = categorias.Where(c => !c.Eliminada);

        if (!string.IsNullOrWhiteSpace(query.Nombre))
        {
            var filtro = query.Nombre.Trim();
            activas = activas.Where(c =>
                c.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase));
        }

        return activas
            .OrderBy(c => c.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(c => c.ToDto())
            .ToList();
    }
}
