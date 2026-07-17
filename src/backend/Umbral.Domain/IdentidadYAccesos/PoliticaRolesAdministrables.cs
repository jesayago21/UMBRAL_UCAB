using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.Shared;

namespace Umbral.Domain.IdentidadYAccesos;

/// <summary>
/// Guarda de dominio para usuarios administrables (HU-42, RB-35).
/// Solo roles Administrador u Operador; exactamente uno por usuario.
/// </summary>
public static class PoliticaRolesAdministrables
{
    /// <summary>Cuenta seed / admin mayor: no eliminable desde el panel.</summary>
    public const string UsernameAdministradorRaiz = "admin";

    public static bool EsAdministradorRaiz(string? username) =>
        string.Equals(
            username?.Trim(),
            UsernameAdministradorRaiz,
            StringComparison.OrdinalIgnoreCase);

    public static void Validar(IReadOnlyList<RolSistema> roles)
    {
        if (roles is null || roles.Count == 0)
            throw new DomainException("Debe indicar al menos un rol.");

        if (roles.Count != 1)
            throw new DomainException("Debe asignarse exactamente un rol.");

        if (roles.Contains(RolSistema.Participante))
        {
            throw new DomainException(
                "El rol Participante no se asigna desde la administración de usuarios (RB-35).");
        }
    }

    public static IReadOnlyList<RolSistema> Parsear(IReadOnlyList<string> roles)
    {
        if (roles is null || roles.Count == 0)
            throw new DomainException("Debe indicar al menos un rol.");

        var parsed = roles
            .Select(r => Enum.Parse<RolSistema>(r, ignoreCase: true))
            .ToList();

        Validar(parsed);
        return parsed;
    }

    /// <summary>
    /// No autoeliminación; la cuenta raíz <c>admin</c> tampoco se elimina.
    /// Otros Administrador u Operador sí pueden eliminarse.
    /// </summary>
    public static void AsegurarPuedeEliminarse(
        string usernameObjetivo,
        Guid keycloakUserIdObjetivo,
        Guid solicitanteKeycloakUserId)
    {
        if (keycloakUserIdObjetivo == solicitanteKeycloakUserId)
        {
            throw new DomainException(
                "No puedes eliminar tu propia cuenta mientras la sesión está activa.");
        }

        if (EsAdministradorRaiz(usernameObjetivo))
        {
            throw new DomainException(
                "No se puede eliminar la cuenta administradora raíz (admin).");
        }
    }
}
