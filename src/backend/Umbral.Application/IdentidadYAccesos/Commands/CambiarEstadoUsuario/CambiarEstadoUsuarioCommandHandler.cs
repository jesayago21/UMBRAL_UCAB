using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Ports;

namespace Umbral.Application.IdentidadYAccesos.Commands.CambiarEstadoUsuario;

internal sealed class CambiarEstadoUsuarioCommandHandler
    : IRequestHandler<CambiarEstadoUsuarioCommand, Result<bool>>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IIdentityService _identity;

    public CambiarEstadoUsuarioCommandHandler(
        IUsuarioRepository usuarios,
        IIdentityService identity)
    {
        _usuarios = usuarios;
        _identity = identity;
    }

    public async Task<Result<bool>> Handle(CambiarEstadoUsuarioCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(new UsuarioAdministrableId(cmd.UsuarioId), ct)
                      ?? throw new NotFoundException(nameof(UsuarioAdministrable), cmd.UsuarioId);

        var accion = cmd.Accion.Trim();
        if (string.Equals(accion, "Activar", StringComparison.OrdinalIgnoreCase))
        {
            usuario.Activar();
            await _identity.CambiarEstadoAsync(usuario.KeycloakUserId, habilitado: true, ct);
        }
        else if (string.Equals(accion, "Bloquear", StringComparison.OrdinalIgnoreCase))
        {
            usuario.Bloquear();
            await _identity.CambiarEstadoAsync(usuario.KeycloakUserId, habilitado: false, ct);
        }
        else
        {
            return Result<bool>.Fail("Acción inválida. Use Activar o Bloquear.");
        }

        await _usuarios.GuardarAsync(usuario, ct);
        return Result<bool>.Ok(true);
    }
}
