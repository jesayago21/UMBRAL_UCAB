using MediatR;
using Umbral.Application.IdentidadYAccesos.Models;
using Umbral.Domain.IdentidadYAccesos.Ports;

namespace Umbral.Application.IdentidadYAccesos.Queries.ListUsuarios;

internal sealed class ListUsuariosQueryHandler
    : IRequestHandler<ListUsuariosQuery, IReadOnlyList<UsuarioDto>>
{
    private readonly IIdentityService _identity;

    public ListUsuariosQueryHandler(IIdentityService identity) => _identity = identity;

    public async Task<IReadOnlyList<UsuarioDto>> Handle(
        ListUsuariosQuery request,
        CancellationToken ct)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var first    = (page - 1) * pageSize;

        var list = await _identity.ListarUsuariosAsync(first, pageSize, ct);
        return list.Select(u => u.ToDto()).ToList();
    }
}
