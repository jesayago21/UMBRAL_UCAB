using MediatR;
using Umbral.Application.Common.Models;

namespace Umbral.Application.CatalogoTrivia.Categorias.Commands.CrearCategoria;

public sealed record CrearCategoriaCommand(string Nombre) : IRequest<Result<Guid>>;
