using MediatR;
using Umbral.Application.Common.Models;
using Umbral.Application.IdentidadYAccesos.Models;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;

namespace Umbral.Application.IdentidadYAccesos.Commands.CrearUsuario;

internal sealed class CrearUsuarioCommandHandler
    : IRequestHandler<CrearUsuarioCommand, Result<CrearUsuarioResult>>
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IIdentityService _identity;

    public CrearUsuarioCommandHandler(IUsuarioRepository usuarios, IIdentityService identity)
    {
        _usuarios = usuarios;
        _identity = identity;
    }

    public async Task<Result<CrearUsuarioResult>> Handle(
        CrearUsuarioCommand cmd,
        CancellationToken ct)
    {
        var email = EmailAddress.Create(cmd.Email);
        var roles = MapearRoles(cmd.Roles);

        if (await _usuarios.ExisteEmailAsync(email, ct))
            return Result<CrearUsuarioResult>.Fail("Email ya registrado (RB-36).");

        if (await _usuarios.ExisteUsernameAsync(cmd.Username, ct))
            return Result<CrearUsuarioResult>.Fail("Username ya registrado (RB-36).");

        KeycloakUserId kcId;
        try
        {
            kcId = await _identity.RegistrarEnIdentityServerAsync(
                email,
                cmd.Username,
                cmd.Nombre,
                cmd.Apellido,
                cmd.PasswordTemporal,
                roles,
                ct);
        }
        catch (Exception ex)
        {
            return Result<CrearUsuarioResult>.Fail($"Identity server: {ex.Message}");
        }

        try
        {
            var usuario = UsuarioAdministrable.Crear(
                kcId,
                email,
                cmd.Username,
                cmd.Nombre,
                cmd.Apellido,
                roles);
            usuario.RegistrarPasswordAsignada(cmd.PasswordTemporal);

            await _usuarios.GuardarAsync(usuario, ct);

            return Result<CrearUsuarioResult>.Ok(
                new CrearUsuarioResult(
                    usuario.Id.Valor,
                    kcId.Value,
                    email.Value,
                    usuario.Estado.ToString()));
        }
        catch
        {
            await _identity.EliminarEnIdentityServerAsync(kcId, ct);
            throw;
        }
    }

    private static IReadOnlyList<RolSistema> MapearRoles(IReadOnlyList<string> roles)
    {
        if (roles is null || roles.Count == 0)
            throw new DomainException("Debe indicar al menos un rol.");

        return roles
            .Select(r => Enum.Parse<RolSistema>(r, ignoreCase: true))
            .ToList();
    }
}
