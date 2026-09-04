using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Application.Tests.Builders;

public static class UsuarioIdentidadTestBuilder
{
    public static UsuarioIdentidad Operador(Guid? keycloakId = null) =>
        new(
            KeycloakUserId.From(keycloakId ?? Guid.NewGuid()),
            "operador@test.com",
            "operador",
            "Oper",
            "Ador",
            EstadoUsuario.Activo,
            [RolSistema.Operador]);

    public static UsuarioIdentidad Administrador(Guid? keycloakId = null, string username = "admin") =>
        new(
            KeycloakUserId.From(keycloakId ?? Guid.NewGuid()),
            $"{username}@test.com",
            username,
            "Admin",
            "User",
            EstadoUsuario.Activo,
            [RolSistema.Administrador]);
}
