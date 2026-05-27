using FluentAssertions;
using Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class CrearSesionBusquedaTesoroValidatorTests
{
    private readonly CrearSesionBusquedaTesoroValidator _validator = new();

    [Fact]
    public void Validar_ComandoCompleto_NoTieneErrores()
    {
        var command = new CrearSesionBusquedaTesoroCommand(
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_MisionIdVacio_TieneError()
    {
        var command = new CrearSesionBusquedaTesoroCommand(
            Guid.Empty,
            Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.MisionId));
    }

    [Fact]
    public void Validar_OperadorIdVacio_TieneError()
    {
        var command = new CrearSesionBusquedaTesoroCommand(
            Guid.NewGuid(),
            Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.OperadorId));
    }
}
