using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Application.IdentidadYAccesos.Commands.CambiarEstadoUsuario;

internal sealed class CambiarEstadoUsuarioCommandHandler
    : IRequestHandler<CambiarEstadoUsuarioCommand, Result<bool>>
{
    private readonly IIdentityService _identity;

    public CambiarEstadoUsuarioCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<Result<bool>> Handle(CambiarEstadoUsuarioCommand cmd, CancellationToken ct)
    {
        var keycloakId = KeycloakUserId.From(cmd.KeycloakUserId);
        _ = await _identity.ObtenerUsuarioPorIdAsync(keycloakId, ct)
            ?? throw new NotFoundException("Usuario", cmd.KeycloakUserId);

        var accion = cmd.Accion.Trim();
        bool habilitado;
        if (string.Equals(accion, "Activar", StringComparison.OrdinalIgnoreCase))
            habilitado = true;
        else if (string.Equals(accion, "Bloquear", StringComparison.OrdinalIgnoreCase))
            habilitado = false;
        else
            return Result<bool>.Fail("Acción inválida. Use Activar o Bloquear.");

        try
        {
            await _identity.CambiarEstadoAsync(keycloakId, habilitado, ct);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Identity server: {ex.Message}");
        }

        return Result<bool>.Ok(true);
    }
}
