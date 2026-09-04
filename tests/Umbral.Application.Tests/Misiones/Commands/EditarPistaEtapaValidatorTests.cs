using FluentAssertions;
using Umbral.Application.Misiones.Commands.EditarPistaEtapa;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class EditarPistaEtapaValidatorTests
{
    private readonly EditarPistaEtapaValidator _sut = new();

    [Fact]
    public void Validate_ConDatosValidos_SinErrores()
    {
        var result = _sut.Validate(new EditarPistaEtapaCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Pista editada",
            "PorGanador",
            null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ContenidoVacio_Error()
    {
        var result = _sut.Validate(new EditarPistaEtapaCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            "PorGanador",
            null));

        result.IsValid.Should().BeFalse();
    }
}
