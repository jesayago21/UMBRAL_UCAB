using MediatR;
using Umbral.Application.IdentidadYAccesos.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Ports;

namespace Umbral.Application.IdentidadYAccesos.Queries.GetUsuarioById;

internal sealed class GetUsuarioByIdQueryHandler : IRequestHandler<GetUsuarioByIdQuery, UsuarioDto?>
{
    private readonly IUsuarioRepository _usuarios;

    public GetUsuarioByIdQueryHandler(IUsuarioRepository usuarios) => _usuarios = usuarios;

    public async Task<UsuarioDto?> Handle(GetUsuarioByIdQuery request, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(
            new UsuarioAdministrableId(request.Id), ct);
        return usuario?.ToDto();
    }
}
