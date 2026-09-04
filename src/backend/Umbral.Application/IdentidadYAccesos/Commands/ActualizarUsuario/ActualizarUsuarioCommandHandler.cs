using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;

namespace Umbral.Application.IdentidadYAccesos.Commands.ActualizarUsuario;

internal sealed class ActualizarUsuarioCommandHandler
    : IRequestHandler<ActualizarUsuarioCommand, Result<bool>>
{
    private readonly IIdentityService _identity;

    public ActualizarUsuarioCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<Result<bool>> Handle(ActualizarUsuarioCommand cmd, CancellationToken ct)
    {
        var keycloakId = KeycloakUserId.From(cmd.KeycloakUserId);
        _ = await _identity.ObtenerUsuarioPorIdAsync(keycloakId, ct)
            ?? throw new NotFoundException("Usuario", cmd.KeycloakUserId);

        IReadOnlyList<Domain.IdentidadYAccesos.Enums.RolSistema> roles;
        try
        {
            roles = PoliticaRolesAdministrables.Parsear([cmd.Rol]);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Fail(ex.Message);
        }

        try
        {
            await _identity.ActualizarPerfilAsync(keycloakId, cmd.Nombre, cmd.Apellido, ct);
            await _identity.SincronizarRolesAsync(keycloakId, roles, ct);

            if (!string.IsNullOrWhiteSpace(cmd.NuevaPassword))
                await _identity.RestablecerPasswordAsync(keycloakId, cmd.NuevaPassword, ct);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Identity server: {ex.Message}");
        }

        return Result<bool>.Ok(true);
    }
}
