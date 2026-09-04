using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Domain.IdentidadYAccesos.Ports;

public interface IIdentityService
{
    Task<KeycloakUserId> RegistrarEnIdentityServerAsync(
        EmailAddress email,
        string username,
        string nombre,
        string apellido,
        IReadOnlyList<RolSistema> roles,
        CancellationToken ct = default);

    /// <summary>
    /// Auto-registro de participante: crea el usuario con contraseña ya definida
    /// (sin required action UPDATE_PASSWORD) y asigna solo el rol Participante.
    /// </summary>
    Task<KeycloakUserId> RegistrarParticipanteEnIdentityServerAsync(
        EmailAddress email,
        string username,
        string nombre,
        string apellido,
        string password,
        CancellationToken ct = default);

    Task SincronizarRolesAsync(
        KeycloakUserId userId,
        IReadOnlyList<RolSistema> roles,
        CancellationToken ct = default);

    Task CambiarEstadoAsync(KeycloakUserId userId, bool habilitado, CancellationToken ct = default);

    Task ActualizarPerfilAsync(
        KeycloakUserId userId,
        string nombre,
        string apellido,
        CancellationToken ct = default);

    Task RestablecerPasswordAsync(
        KeycloakUserId userId,
        string nuevaPassword,
        CancellationToken ct = default);

    Task EliminarEnIdentityServerAsync(KeycloakUserId userId, CancellationToken ct = default);

    Task<KeycloakUserId?> ObtenerIdPorUsernameAsync(string username, CancellationToken ct = default);

    Task<bool> ExisteEmailAsync(EmailAddress email, CancellationToken ct = default);

    Task<bool> ExisteUsernameAsync(string username, CancellationToken ct = default);

    Task<UsuarioIdentidad?> ObtenerUsuarioPorIdAsync(KeycloakUserId userId, CancellationToken ct = default);

    Task<IReadOnlyList<UsuarioIdentidad>> ListarUsuariosAsync(
        int first,
        int max,
        CancellationToken ct = default);

    Task<IReadOnlyList<RolSistema>> ObtenerRolesAsync(
        KeycloakUserId userId,
        CancellationToken ct = default);
}
