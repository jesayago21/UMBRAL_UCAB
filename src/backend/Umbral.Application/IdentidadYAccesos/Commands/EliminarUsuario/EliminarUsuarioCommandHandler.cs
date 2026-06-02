using MediatR;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Common.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.Shared;

namespace Umbral.Application.IdentidadYAccesos.Commands.EliminarUsuario;

internal sealed class EliminarUsuarioCommandHandler
    : IRequestHandler<EliminarUsuarioCommand, Result<bool>>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IIdentityService _identity;

    public EliminarUsuarioCommandHandler(IUsuarioRepository usuarios, IIdentityService identity)
    {
        _usuarios = usuarios;
        _identity = identity;
    }

    public async Task<Result<bool>> Handle(EliminarUsuarioCommand cmd, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(new UsuarioAdministrableId(cmd.UsuarioId), ct)
                      ?? throw new NotFoundException(nameof(UsuarioAdministrable), cmd.UsuarioId);

        try
        {
            usuario.AsegurarPuedeEliminarse();
        }
        catch (DomainException ex)
        {
            return Result<bool>.Fail(ex.Message);
        }

        try
        {
            await _identity.EliminarEnIdentityServerAsync(usuario.KeycloakUserId, ct);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Identity server: {ex.Message}");
        }

        await _usuarios.EliminarAsync(usuario, ct);
        return Result<bool>.Ok(true);
    }
}
