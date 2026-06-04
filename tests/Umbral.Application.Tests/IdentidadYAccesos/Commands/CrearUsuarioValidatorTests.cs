using FluentAssertions;
using Umbral.Application.IdentidadYAccesos.Commands.CrearUsuario;
using Xunit;

namespace Umbral.Application.Tests.IdentidadYAccesos.Commands;

public sealed class CrearUsuarioValidatorTests
{
    private readonly CrearUsuarioValidator _sut = new();

    [Fact]
    public void Validate_ConDatosValidos_SinErrores()
    {
        var result = _sut.Validate(new CrearUsuarioCommand(
            "valido@umbral.test",
            "valido_user",
            "Nombre",
            "Apellido",
            "Password1",
            ["Operador"]));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmailInvalido_Error()
    {
        var result = _sut.Validate(new CrearUsuarioCommand(
            "no-es-email",
            "user",
            "Nombre",
            "Apellido",
            "Password1",
            ["Operador"]));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_PasswordCorta_Error()
    {
        var result = _sut.Validate(new CrearUsuarioCommand(
            "valido@umbral.test",
            "user",
            "Nombre",
            "Apellido",
            "corta",
            ["Operador"]));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_VariosRoles_Error()
    {
        var result = _sut.Validate(new CrearUsuarioCommand(
            "valido@umbral.test",
            "user",
            "Nombre",
            "Apellido",
            "Password1",
            ["Administrador", "Operador"]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("exactamente un rol"));
    }
}
