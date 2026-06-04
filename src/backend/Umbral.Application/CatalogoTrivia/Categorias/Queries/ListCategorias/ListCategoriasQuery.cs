using MediatR;
using Umbral.Application.CatalogoTrivia.Models;

namespace Umbral.Application.CatalogoTrivia.Categorias.Queries.ListCategorias;

public sealed record ListCategoriasQuery(string? Nombre = null) : IRequest<IReadOnlyList<CategoriaDto>>;
