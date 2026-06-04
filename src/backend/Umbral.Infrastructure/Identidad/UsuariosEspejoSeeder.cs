using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Ports;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Infrastructure.Identidad;

/// <summary>
/// Replica en PostgreSQL los usuarios demo del realm Keycloak (admin, operador, participante).
/// </summary>
public sealed class UsuariosEspejoSeeder
{
    private sealed record UsuarioDemoDef(
        string Username,
        string Email,
        string Nombre,
        string Apellido,
        RolSistema Rol,
        string Password);

    private static readonly UsuarioDemoDef[] UsuariosDemo =
    [
        new("admin", "admin@umbral.local", "Admin", "UMBRAL", RolSistema.Administrador, "Umbral123!"),
        new("operador", "operador@umbral.local", "Operador", "UMBRAL", RolSistema.Operador, "Umbral123!"),
        new(
            "participante",
            "participante@umbral.local",
            "Participante",
            "UMBRAL",
            RolSistema.Participante,
            "Umbral123!")
    ];

    private readonly IUsuarioRepository _usuarios;
    private readonly IIdentityService _identity;
    private readonly KeycloakAdminOptions _options;
    private readonly ILogger<UsuariosEspejoSeeder> _logger;

    public UsuariosEspejoSeeder(
        IUsuarioRepository usuarios,
        IIdentityService identity,
        IOptions<KeycloakAdminOptions> options,
        ILogger<UsuariosEspejoSeeder> logger)
    {
        _usuarios  = usuarios;
        _identity  = identity;
        _options   = options.Value;
        _logger    = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (_options.UseDevStub)
        {
            _logger.LogWarning(
                "KeycloakAdmin.UseDevStub=true: no se sincronizan usuarios demo al espejo. " +
                "Use UseDevStub=false con Keycloak en marcha.");
            return;
        }

        foreach (var demo in UsuariosDemo)
        {
            var existente = await _usuarios.ObtenerPorUsernameAsync(demo.Username, ct);
            if (existente is not null)
            {
                if (string.IsNullOrEmpty(existente.PasswordAsignada))
                {
                    existente.RegistrarPasswordAsignada(demo.Password);
                    await _usuarios.GuardarAsync(existente, ct);
                }

                continue;
            }

            var kcId = await _identity.ObtenerIdPorUsernameAsync(demo.Username, ct);
            if (kcId is null)
            {
                _logger.LogWarning(
                    "Usuario demo «{Username}» no encontrado en Keycloak; omitiendo espejo.",
                    demo.Username);
                continue;
            }

            var usuario = UsuarioAdministrable.Crear(
                kcId,
                EmailAddress.Create(demo.Email),
                demo.Username,
                demo.Nombre,
                demo.Apellido,
                [demo.Rol]);
            usuario.RegistrarPasswordAsignada(demo.Password);
            await _usuarios.GuardarAsync(usuario, ct);
            _logger.LogInformation("Usuario demo «{Username}» registrado en tabla espejo.", demo.Username);
        }
    }
}
