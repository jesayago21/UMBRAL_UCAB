using MediatR;
using Umbral.Application.IdentidadYAccesos.Models;

namespace Umbral.Application.IdentidadYAccesos.Queries.ListUsuarios;

public sealed record ListUsuariosQuery(int Page = 1, int PageSize = 50)
    : IRequest<IReadOnlyList<UsuarioDto>>;
