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
    public void AsegurarPuedeEliminarse_Administrador_LanzaDomainException()
    {
        var act = () => PoliticaRolesAdministrables.AsegurarPuedeEliminarse([RolSistema.Administrador]);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AsegurarPuedeEliminarse_Operador_NoLanza()
    {
        var act = () => PoliticaRolesAdministrables.AsegurarPuedeEliminarse([RolSistema.Operador]);
        act.Should().NotThrow();
    }
}
