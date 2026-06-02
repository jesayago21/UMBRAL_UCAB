using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Domain.IdentidadYAccesos.Ports;

public interface IUsuarioRepository
{
    Task<UsuarioAdministrable?> ObtenerPorIdAsync(UsuarioAdministrableId id, CancellationToken ct = default);
    Task<UsuarioAdministrable?> ObtenerPorUsernameAsync(string username, CancellationToken ct = default);
    Task<UsuarioAdministrable?> ObtenerPorEmailAsync(EmailAddress email, CancellationToken ct = default);
    Task<UsuarioAdministrable?> ObtenerPorKeycloakIdAsync(KeycloakUserId id, CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(EmailAddress email, CancellationToken ct = default);
    Task<bool> ExisteUsernameAsync(string username, CancellationToken ct = default);
    Task GuardarAsync(UsuarioAdministrable usuario, CancellationToken ct = default);
    Task EliminarAsync(UsuarioAdministrable usuario, CancellationToken ct = default);
    Task<IReadOnlyList<UsuarioAdministrable>> ListarAsync(int skip, int take, CancellationToken ct = default);
}
