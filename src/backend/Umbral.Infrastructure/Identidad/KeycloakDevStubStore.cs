using System.Collections.Concurrent;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Infrastructure.Identidad;

/// <summary>
/// Almacén en memoria para simular Keycloak cuando UseDevStub=true (tests e integración local).
/// </summary>
internal static class KeycloakDevStubStore
{
    private static readonly ConcurrentDictionary<Guid, UsuarioIdentidad> Users = new();

    public static KeycloakUserId Registrar(
        EmailAddress email,
        string username,
        string nombre,
        string apellido,
        IReadOnlyList<RolSistema> roles)
    {
        var id = KeycloakUserId.From(Guid.NewGuid());
        var usuario = new UsuarioIdentidad(
            id,
            email.Value,
            username.Trim(),
            nombre.Trim(),
            apellido.Trim(),
            EstadoUsuario.Activo,
            roles.ToList());

        Users[id.Value] = usuario;
        return id;
    }

    public static bool ExisteEmail(EmailAddress email) =>
        Users.Values.Any(u =>
            string.Equals(u.Email, email.Value, StringComparison.OrdinalIgnoreCase));

    public static bool ExisteUsername(string username) =>
        Users.Values.Any(u =>
            string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));

    public static UsuarioIdentidad? ObtenerPorId(KeycloakUserId userId) =>
        Users.TryGetValue(userId.Value, out var user) ? user : null;

    public static KeycloakUserId? ObtenerIdPorUsername(string username)
    {
        var match = Users.Values.FirstOrDefault(u =>
            string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
        return match?.KeycloakUserId;
    }

    public static IReadOnlyList<UsuarioIdentidad> Listar(int first, int max) =>
        Users.Values
            .OrderBy(u => u.Username, StringComparer.OrdinalIgnoreCase)
            .Skip(Math.Max(0, first))
            .Take(Math.Clamp(max, 1, 100))
            .ToList();

    public static IReadOnlyList<RolSistema> ObtenerRoles(KeycloakUserId userId) =>
        ObtenerPorId(userId)?.Roles ?? [];

    public static void SincronizarRoles(KeycloakUserId userId, IReadOnlyList<RolSistema> roles)
    {
        if (!Users.TryGetValue(userId.Value, out var user))
            return;

        Users[userId.Value] = user with { Roles = roles.ToList() };
    }

    public static void CambiarEstado(KeycloakUserId userId, bool habilitado)
    {
        if (!Users.TryGetValue(userId.Value, out var user))
            return;

        Users[userId.Value] = user with
        {
            Estado = habilitado ? EstadoUsuario.Activo : EstadoUsuario.Bloqueado
        };
    }

    public static void ActualizarPerfil(KeycloakUserId userId, string nombre, string apellido)
    {
        if (!Users.TryGetValue(userId.Value, out var user))
            return;

        Users[userId.Value] = user with
        {
            Nombre = nombre.Trim(),
            Apellido = apellido.Trim()
        };
    }

    public static void Eliminar(KeycloakUserId userId) =>
        Users.TryRemove(userId.Value, out _);
}
