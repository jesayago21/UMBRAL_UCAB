using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;

namespace Umbral.Application.IdentidadYAccesos.Commands.EliminarUsuario;

internal sealed class EliminarUsuarioCommandHandler
    : IRequestHandler<EliminarUsuarioCommand, Result<bool>>
{
    private readonly IIdentityService _identity;

    public EliminarUsuarioCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<Result<bool>> Handle(EliminarUsuarioCommand cmd, CancellationToken ct)
    {
        var keycloakId = KeycloakUserId.From(cmd.KeycloakUserId);
        var usuario = await _identity.ObtenerUsuarioPorIdAsync(keycloakId, ct)
                      ?? throw new NotFoundException("Usuario", cmd.KeycloakUserId);

        try
        {
            PoliticaRolesAdministrables.AsegurarPuedeEliminarse(usuario.Roles);
        }
        catch (DomainException ex)
        {
            return Result<bool>.Fail(ex.Message);
        }

        try
        {
            await _identity.EliminarEnIdentityServerAsync(keycloakId, ct);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Identity server: {ex.Message}");
        }

        return Result<bool>.Ok(true);
    }
}
