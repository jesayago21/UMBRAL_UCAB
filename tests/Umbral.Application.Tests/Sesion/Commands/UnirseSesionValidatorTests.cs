using FluentAssertions;
using Umbral.Application.Sesion.Commands.UnirseSesion;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class UnirseSesionValidatorTests
{
    private readonly UnirseSesionValidator _validator = new();

    [Fact]
    public void Validar_ComandoCompleto_NoTieneErrores()
    {
        var command = new UnirseSesionCommand(
            Guid.NewGuid(),
            "ABC123",
            Guid.NewGuid(),
            "Alpha");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_SesionIdVacio_TieneError()
    {
        var command = new UnirseSesionCommand(
            Guid.Empty,
            "ABC123",
            Guid.NewGuid(),
            "Alpha");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.SesionId));
    }

    [Fact]
    public void Validar_CodigoAccesoVacio_TieneError()
    {
        var command = new UnirseSesionCommand(
            Guid.NewGuid(),
            "",
            Guid.NewGuid(),
            "Alpha");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CodigoAcceso));
    }

    [Fact]
    public void Validar_JugadorIdVacio_TieneError()
    {
        var command = new UnirseSesionCommand(
            Guid.NewGuid(),
            "ABC123",
            Guid.Empty,
            "Alpha");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.JugadorId));
    }

    [Fact]
    public void Validar_NombreParticipanteVacio_TieneError()
    {
        var command = new UnirseSesionCommand(
            Guid.NewGuid(),
            "ABC123",
            Guid.NewGuid(),
            "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.NombreParticipante));
    }
}
