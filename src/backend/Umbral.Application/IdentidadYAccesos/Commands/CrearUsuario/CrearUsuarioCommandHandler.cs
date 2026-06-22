using MediatR;
using Umbral.Application.Common.Models;
using Umbral.Application.IdentidadYAccesos.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;

namespace Umbral.Application.IdentidadYAccesos.Commands.CrearUsuario;

internal sealed class CrearUsuarioCommandHandler
    : IRequestHandler<CrearUsuarioCommand, Result<CrearUsuarioResult>>
{
    private readonly IIdentityService _identity;

    public CrearUsuarioCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<Result<CrearUsuarioResult>> Handle(
        CrearUsuarioCommand cmd,
        CancellationToken ct)
    {
        var email = EmailAddress.Create(cmd.Email);

        IReadOnlyList<Domain.IdentidadYAccesos.Enums.RolSistema> roles;
        try
        {
            roles = PoliticaRolesAdministrables.Parsear(cmd.Roles);
        }
        catch (DomainException ex)
        {
            return Result<CrearUsuarioResult>.Fail(ex.Message);
        }

        if (await _identity.ExisteEmailAsync(email, ct))
            return Result<CrearUsuarioResult>.Fail("Email ya registrado (RB-36).");

        if (await _identity.ExisteUsernameAsync(cmd.Username, ct))
            return Result<CrearUsuarioResult>.Fail("Username ya registrado (RB-36).");

        try
        {
            var kcId = await _identity.RegistrarEnIdentityServerAsync(
                email,
                cmd.Username,
                cmd.Nombre,
                cmd.Apellido,
                cmd.PasswordTemporal,
                roles,
                ct);

            return Result<CrearUsuarioResult>.Ok(
                new CrearUsuarioResult(
                    kcId.Value,
                    email.Value,
                    Domain.IdentidadYAccesos.Enums.EstadoUsuario.Activo.ToString(),
                    roles.Select(r => r.ToString()).ToList()));
        }
        catch (Exception ex)
        {
            return Result<CrearUsuarioResult>.Fail($"Identity server: {ex.Message}");
        }
    }
}
