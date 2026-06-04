using MediatR;
using Umbral.Application.CatalogoTrivia.Models;

namespace Umbral.Application.CatalogoTrivia.Categorias.Queries.GetCategoriaById;

public sealed record GetCategoriaByIdQuery(Guid CategoriaId) : IRequest<CategoriaDto>;
