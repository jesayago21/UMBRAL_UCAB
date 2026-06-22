using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;

namespace Umbral.Application.IdentidadYAccesos.Commands.AsignarRolesUsuario;

internal sealed class AsignarRolesUsuarioCommandHandler
    : IRequestHandler<AsignarRolesUsuarioCommand, Result<bool>>
{
    private readonly IIdentityService _identity;

    public AsignarRolesUsuarioCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<Result<bool>> Handle(AsignarRolesUsuarioCommand cmd, CancellationToken ct)
    {
        var keycloakId = KeycloakUserId.From(cmd.KeycloakUserId);
        _ = await _identity.ObtenerUsuarioPorIdAsync(keycloakId, ct)
            ?? throw new NotFoundException("Usuario", cmd.KeycloakUserId);

        IReadOnlyList<Domain.IdentidadYAccesos.Enums.RolSistema> roles;
        try
        {
            roles = PoliticaRolesAdministrables.Parsear(cmd.Roles);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Fail(ex.Message);
        }

        try
        {
            await _identity.SincronizarRolesAsync(keycloakId, roles, ct);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Identity server: {ex.Message}");
        }

        return Result<bool>.Ok(true);
    }
}
