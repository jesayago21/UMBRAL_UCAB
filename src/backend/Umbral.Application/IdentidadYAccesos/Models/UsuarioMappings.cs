using Umbral.Domain.IdentidadYAccesos;

namespace Umbral.Application.IdentidadYAccesos.Models;

internal static class UsuarioMappings
{
    public static UsuarioDto ToDto(this UsuarioIdentidad usuario) =>
        new(
            usuario.KeycloakUserId.Value,
            usuario.Email,
            usuario.Username,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Estado.ToString(),
            usuario.Roles.Select(r => r.ToString()).ToList());
}
