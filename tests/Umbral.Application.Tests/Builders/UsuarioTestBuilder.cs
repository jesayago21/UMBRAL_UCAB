using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Application.Tests.Builders;

internal static class UsuarioTestBuilder
{
    public static UsuarioAdministrable Administrador(string email = "admin@umbral.test")
    {
        var usuario = UsuarioAdministrable.Crear(
            KeycloakUserId.From(Guid.NewGuid()),
            EmailAddress.Create(email),
            email.Split('@')[0],
            "Admin",
            "Test",
            [RolSistema.Administrador]);
        usuario.ClearDomainEvents();
        return usuario;
    }

    public static UsuarioAdministrable Operador(string email = "operador@umbral.test")
    {
        var usuario = UsuarioAdministrable.Crear(
            KeycloakUserId.From(Guid.NewGuid()),
            EmailAddress.Create(email),
            email.Split('@')[0],
            "Operador",
            "Test",
            [RolSistema.Operador]);
        usuario.ClearDomainEvents();
        return usuario;
    }
}
