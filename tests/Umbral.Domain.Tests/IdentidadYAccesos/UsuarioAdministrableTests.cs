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
    public void Crear_ConRolOperador_EstadoActivoYEventoUsuarioCreado()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId,
            Email,
            "admin_test",
            "Admin",
            "Test",
            [RolSistema.Operador]);

        usuario.Estado.Should().Be(EstadoUsuario.Activo);
        usuario.Roles.Should().ContainSingle().Which.Should().Be(RolSistema.Operador);
        usuario.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UsuarioCreadoEnDominio>();
    }

    [Fact]
    public void Crear_ConVariosRoles_LanzaDomainException()
    {
        var act = () => UsuarioAdministrable.Crear(
            KcId, Email, "mix", "M", "T", [RolSistema.Administrador, RolSistema.Operador]);

        act.Should().Throw<DomainException>()
            .WithMessage("*exactamente un rol*");
    }

    [Fact]
    public void Crear_ConRolParticipante_AceptaParticipante()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId, Email, "part", "P", "T", [RolSistema.Participante]);

        usuario.Roles.Should().ContainSingle().Which.Should().Be(RolSistema.Participante);
    }

    [Fact]
    public void Crear_SinRoles_LanzaDomainException()
    {
        var act = () => UsuarioAdministrable.Crear(
            KcId, Email, "user", "U", "T", []);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AsignarRoles_ReemplazaRolYEmiteRolesUsuarioModificados()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId, Email, "op", "Op", "T", [RolSistema.Operador]);
        usuario.ClearDomainEvents();

        usuario.AsignarRoles([RolSistema.Administrador]);

        usuario.Roles.Should().ContainSingle().Which.Should().Be(RolSistema.Administrador);
        usuario.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RolesUsuarioModificados>();
    }

    [Fact]
    public void AsegurarPuedeEliminarse_ConAdministrador_LanzaDomainException()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId, Email, "adm", "Adm", "T", [RolSistema.Administrador]);

        var act = () => usuario.AsegurarPuedeEliminarse();

        act.Should().Throw<DomainException>();
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

    [Fact]
    public void RevocarRol_CuandoEsElUnicoRol_LanzaDomainException()
    {
        var usuario = UsuarioAdministrable.Crear(
            KcId, Email, "op_solo", "Op", "T", [RolSistema.Operador]);

        var act = () => usuario.RevocarRol(RolSistema.Operador);

        act.Should().Throw<DomainException>()
            .WithMessage("*al menos un rol*");
    }
}
