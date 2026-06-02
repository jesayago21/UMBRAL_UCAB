using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.Shared;

namespace Umbral.Application.IdentidadYAccesos.Commands.AsignarRolesUsuario;

internal sealed class AsignarRolesUsuarioCommandHandler
    : IRequestHandler<AsignarRolesUsuarioCommand, Result<bool>>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IIdentityService _identity;

    public AsignarRolesUsuarioCommandHandler(
        IUsuarioRepository usuarios,
        IIdentityService identity)
    {
        _usuarios = usuarios;
        _identity = identity;
    }

    public async Task<Result<bool>> Handle(AsignarRolesUsuarioCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(new UsuarioAdministrableId(cmd.UsuarioId), ct)
                      ?? throw new NotFoundException(nameof(UsuarioAdministrable), cmd.UsuarioId);

        var roles = cmd.Roles.Select(r => Enum.Parse<RolSistema>(r, ignoreCase: true)).ToList();
        usuario.AsignarRoles(roles);
        await _identity.SincronizarRolesAsync(usuario.KeycloakUserId, roles, ct);
        await _usuarios.GuardarAsync(usuario, ct);
        return Result<bool>.Ok(true);
    }
}
