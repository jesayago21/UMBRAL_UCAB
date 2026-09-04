using MediatR;
using Umbral.Application.IdentidadYAccesos.Models;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Application.IdentidadYAccesos.Queries.GetUsuarioById;

internal sealed class GetUsuarioByIdQueryHandler : IRequestHandler<GetUsuarioByIdQuery, UsuarioDto?>
{
    private readonly IIdentityService _identity;

    public GetUsuarioByIdQueryHandler(IIdentityService identity) => _identity = identity;

    public async Task<UsuarioDto?> Handle(GetUsuarioByIdQuery request, CancellationToken ct)
    {
        var usuario = await _identity.ObtenerUsuarioPorIdAsync(
            KeycloakUserId.From(request.KeycloakUserId), ct);
        return usuario?.ToDto();
    }
}
