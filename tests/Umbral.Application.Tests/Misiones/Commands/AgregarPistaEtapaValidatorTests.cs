using FluentAssertions;
using Umbral.Application.Misiones.Commands.AgregarPistaEtapa;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class AgregarPistaEtapaValidatorTests
{
    private readonly AgregarPistaEtapaValidator _sut = new();

    [Fact]
    public void Validate_ConDatosValidos_SinErrores()
    {
        var result = _sut.Validate(new AgregarPistaEtapaCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Pista",
            "PorTiempo",
            30));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_TipoLiberacionInvalido_Error()
    {
        var result = _sut.Validate(new AgregarPistaEtapaCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Pista",
            "Manual",
            null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_PorTiempoConSegundosValidos_SinErrores()
    {
        var result = _sut.Validate(new AgregarPistaEtapaCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Pista",
            "PorTiempo",
            30));

        result.IsValid.Should().BeTrue();
    }
}
