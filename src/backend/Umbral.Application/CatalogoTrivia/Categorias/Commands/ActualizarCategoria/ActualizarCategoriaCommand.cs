using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.CatalogoTrivia.Categorias.Commands.ActualizarCategoria;

public sealed record ActualizarCategoriaCommand(
    Guid CategoriaId,
    string Nombre) : IRequest<Result<Guid>>;
