using FluentAssertions;
using Umbral.Application.Misiones.Commands.EliminarPistaEtapa;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class EliminarPistaEtapaValidatorTests
{
    private readonly EliminarPistaEtapaValidator _sut = new();

    [Fact]
    public void Validate_ConDatosValidos_SinErrores()
    {
        var result = _sut.Validate(new EliminarPistaEtapaCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_PistaIdVacio_Error()
    {
        var result = _sut.Validate(new EliminarPistaEtapaCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty));

        result.IsValid.Should().BeFalse();
    }
}
