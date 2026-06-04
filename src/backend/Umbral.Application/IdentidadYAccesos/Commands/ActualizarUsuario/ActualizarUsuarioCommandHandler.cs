using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;

namespace Umbral.Application.IdentidadYAccesos.Commands.ActualizarUsuario;

internal sealed class ActualizarUsuarioCommandHandler
    : IRequestHandler<ActualizarUsuarioCommand, Result<bool>>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IIdentityService _identity;

    public ActualizarUsuarioCommandHandler(IUsuarioRepository usuarios, IIdentityService identity)
    {
        _usuarios = usuarios;
        _identity = identity;
    }

    public async Task<Result<bool>> Handle(ActualizarUsuarioCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(new UsuarioAdministrableId(cmd.UsuarioId), ct)
                      ?? throw new NotFoundException(nameof(UsuarioAdministrable), cmd.UsuarioId);

        usuario.ActualizarPerfil(cmd.Nombre, cmd.Apellido);
        var rol = Enum.Parse<RolSistema>(cmd.Rol, ignoreCase: true);
        usuario.AsignarRoles([rol]);

        try
        {
            await _identity.ActualizarPerfilAsync(
                usuario.KeycloakUserId,
                usuario.Nombre,
                usuario.Apellido,
                ct);
            await _identity.SincronizarRolesAsync(usuario.KeycloakUserId, [rol], ct);

            if (!string.IsNullOrWhiteSpace(cmd.NuevaPassword))
            {
                await _identity.RestablecerPasswordAsync(
                    usuario.KeycloakUserId,
                    cmd.NuevaPassword,
                    ct);
                usuario.RegistrarPasswordAsignada(cmd.NuevaPassword);
            }
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Identity server: {ex.Message}");
        }

        await _usuarios.GuardarAsync(usuario, ct);
        return Result<bool>.Ok(true);
    }
}
