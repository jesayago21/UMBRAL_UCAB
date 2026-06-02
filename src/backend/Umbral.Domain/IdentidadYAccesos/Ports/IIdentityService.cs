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
        string passwordTemporal,
        IReadOnlyList<RolSistema> roles,
        CancellationToken ct = default);

    Task SincronizarRolesAsync(
        KeycloakUserId userId,
        IReadOnlyList<RolSistema> roles,
        CancellationToken ct = default);

    Task CambiarEstadoAsync(KeycloakUserId userId, bool habilitado, CancellationToken ct = default);

    Task EliminarEnIdentityServerAsync(KeycloakUserId userId, CancellationToken ct = default);
}
