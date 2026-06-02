using FluentAssertions;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Events;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.IdentidadYAccesos;

public sealed class UsuarioAdministrableTests
{
    private static KeycloakUserId KcId => KeycloakUserId.From(Guid.NewGuid());
    private static EmailAddress Email => EmailAddress.Create("admin@umbral.test");

    [Fact]
    public void Crear_ConRolesAdministradorYOperador_EstadoActivoYEventoUsuarioCreado()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId,
            Email,
            "admin_test",
            "Admin",
            "Test",
            [RolSistema.Administrador, RolSistema.Operador]);

        usuario.Estado.Should().Be(EstadoUsuario.Activo);
        usuario.Roles.Should().BeEquivalentTo([RolSistema.Administrador, RolSistema.Operador]);
        usuario.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UsuarioCreadoEnDominio>();
    }

    [Fact]
    public void Crear_ConRolParticipante_LanzaDomainException()
    {
        var act = () => UsuarioAdministrable.Crear(
            KcId, Email, "part", "P", "T", [RolSistema.Participante]);

        act.Should().Throw<DomainException>()
            .WithMessage("*RB-35*");
    }

    [Fact]
    public void Crear_SinRoles_LanzaDomainException()
    {
        var act = () => UsuarioAdministrable.Crear(
            KcId, Email, "user", "U", "T", []);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AsignarRoles_ReemplazaRolesYEmiteRolesUsuarioModificados()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId, Email, "op", "Op", "T", [RolSistema.Operador]);
        usuario.ClearDomainEvents();

        usuario.AsignarRoles([RolSistema.Administrador, RolSistema.Administrador]);

        usuario.Roles.Should().ContainSingle().Which.Should().Be(RolSistema.Administrador);
        usuario.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RolesUsuarioModificados>();
    }

    [Fact]
    public void RevocarRol_EmiteRolesUsuarioModificados()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId, Email, "mix", "Mix", "T", [RolSistema.Administrador, RolSistema.Operador]);
        usuario.ClearDomainEvents();

        usuario.RevocarRol(RolSistema.Operador);

        usuario.Roles.Should().ContainSingle().Which.Should().Be(RolSistema.Administrador);
        usuario.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RolesUsuarioModificados>();
    }

    [Fact]
    public void Bloquear_CuandoActivo_CambiaEstadoYEvento()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId, Email, "u1", "U", "T", [RolSistema.Administrador]);
        usuario.ClearDomainEvents();

        usuario.Bloquear();

        usuario.Estado.Should().Be(EstadoUsuario.Bloqueado);
        usuario.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UsuarioEstadoCambiado>();
    }

    [Fact]
    public void Bloquear_CuandoYaBloqueado_EsIdempotente()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId, Email, "u2", "U", "T", [RolSistema.Administrador]);
        usuario.Bloquear();
        usuario.ClearDomainEvents();

        usuario.Bloquear();

        usuario.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Activar_CuandoBloqueado_RestauraActivo()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId, Email, "u3", "U", "T", [RolSistema.Administrador]);
        usuario.Bloquear();
        usuario.ClearDomainEvents();

        usuario.Activar();

        usuario.Estado.Should().Be(EstadoUsuario.Activo);
        usuario.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UsuarioEstadoCambiado>();
    }
}
