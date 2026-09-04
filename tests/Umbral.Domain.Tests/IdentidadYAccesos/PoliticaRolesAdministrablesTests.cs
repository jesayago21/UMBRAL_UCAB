using FluentAssertions;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.IdentidadYAccesos;

public sealed class PoliticaRolesAdministrablesTests
{
    [Fact]
    public void Validar_UnRolAdministrador_NoLanza()
    {
        var act = () => PoliticaRolesAdministrables.Validar([RolSistema.Administrador]);
        act.Should().NotThrow();
    }

    [Fact]
    public void Validar_SinRoles_LanzaDomainException()
    {
        var act = () => PoliticaRolesAdministrables.Validar([]);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validar_MasDeUnRol_LanzaDomainException()
    {
        var act = () => PoliticaRolesAdministrables.Validar([RolSistema.Administrador, RolSistema.Operador]);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validar_RolParticipante_LanzaDomainException_RB35()
    {
        var act = () => PoliticaRolesAdministrables.Validar([RolSistema.Participante]);
        act.Should().Throw<DomainException>()
            .WithMessage("*RB-35*");
    }

    [Fact]
    public void Parsear_RolParticipanteEnString_LanzaDomainException()
    {
        var act = () => PoliticaRolesAdministrables.Parsear(["Participante"]);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AsegurarPuedeEliminarse_AdminRaiz_LanzaDomainException()
    {
        var act = () => PoliticaRolesAdministrables.AsegurarPuedeEliminarse(
            "admin", Guid.NewGuid(), Guid.NewGuid());
        act.Should().Throw<DomainException>()
            .WithMessage("*admin*");
    }

    [Fact]
    public void AsegurarPuedeEliminarse_AutoEliminacion_LanzaDomainException()
    {
        var id = Guid.NewGuid();
        var act = () => PoliticaRolesAdministrables.AsegurarPuedeEliminarse("admin2", id, id);
        act.Should().Throw<DomainException>()
            .WithMessage("*propia cuenta*");
    }

    [Fact]
    public void AsegurarPuedeEliminarse_OtroAdministrador_NoLanza()
    {
        var act = () => PoliticaRolesAdministrables.AsegurarPuedeEliminarse(
            "admin2", Guid.NewGuid(), Guid.NewGuid());
        act.Should().NotThrow();
    }

    [Fact]
    public void AsegurarPuedeEliminarse_Operador_NoLanza()
    {
        var act = () => PoliticaRolesAdministrables.AsegurarPuedeEliminarse(
            "operador", Guid.NewGuid(), Guid.NewGuid());
        act.Should().NotThrow();
    }

    [Fact]
    public void EsAdministradorRaiz_IgnoraMayusculas()
    {
        PoliticaRolesAdministrables.EsAdministradorRaiz("Admin").Should().BeTrue();
        PoliticaRolesAdministrables.EsAdministradorRaiz("admin2").Should().BeFalse();
    }
}
