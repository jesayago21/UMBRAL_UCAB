using MediatR;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.Common.Exceptions;
using Umbral.Domain.CatalogoTrivia.Categoria;

namespace Umbral.Application.CatalogoTrivia.Categorias.Queries.GetCategoriaById;

internal sealed class GetCategoriaByIdQueryHandler : IRequestHandler<GetCategoriaByIdQuery, CategoriaDto>
{
    private readonly ICategoriaRepository _categoriaRepository;

    public GetCategoriaByIdQueryHandler(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<CategoriaDto> Handle(GetCategoriaByIdQuery query, CancellationToken cancellationToken)
    {
        var categoria = await _categoriaRepository.FindByIdAsync(
            new CategoriaId(query.CategoriaId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), query.CategoriaId);

        if (categoria.Eliminada)
            throw new NotFoundException(nameof(Categoria), query.CategoriaId);

        return categoria.ToDto();
    }
}
