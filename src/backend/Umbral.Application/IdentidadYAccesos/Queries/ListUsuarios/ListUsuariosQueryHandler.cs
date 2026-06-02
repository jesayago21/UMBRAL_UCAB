using MediatR;
using Umbral.Application.IdentidadYAccesos.Models;
using Umbral.Domain.IdentidadYAccesos.Ports;

namespace Umbral.Application.IdentidadYAccesos.Queries.ListUsuarios;

internal sealed class ListUsuariosQueryHandler
    : IRequestHandler<ListUsuariosQuery, IReadOnlyList<UsuarioDto>>
{
    private readonly IUsuarioRepository _usuarios;

    public ListUsuariosQueryHandler(IUsuarioRepository usuarios) => _usuarios = usuarios;

    public async Task<IReadOnlyList<UsuarioDto>> Handle(
        ListUsuariosQuery request,
        CancellationToken ct)
    {
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip     = (page - 1) * pageSize;

        var list = await _usuarios.ListarAsync(skip, pageSize, ct);
        return list.Select(u => u.ToDto()).ToList();
    }
}
