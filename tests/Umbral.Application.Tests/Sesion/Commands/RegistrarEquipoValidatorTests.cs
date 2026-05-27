using FluentAssertions;
using Umbral.Application.Sesion.Commands.RegistrarEquipo;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class RegistrarEquipoValidatorTests
{
    private readonly RegistrarEquipoValidator _validator = new();

    [Fact]
    public void Validar_ComandoCompleto_NoTieneErrores()
    {
        var command = new RegistrarEquipoCommand(Guid.NewGuid(), "Alpha");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_SesionIdVacio_TieneError()
    {
        var command = new RegistrarEquipoCommand(Guid.Empty, "Alpha");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.SesionId));
    }

    [Fact]
    public void Validar_NombreEquipoVacio_TieneError()
    {
        var command = new RegistrarEquipoCommand(Guid.NewGuid(), "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.NombreEquipo));
    }
}
