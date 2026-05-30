using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.CatalogoTrivia.Categorias.Commands.EliminarCategoria;

public sealed record EliminarCategoriaCommand(Guid CategoriaId) : IRequest<Result<Guid>>;
