using FluentAssertions;
using Umbral.Application.Sesion.Commands.AplicarPenalizacion;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class AplicarPenalizacionValidatorTests
{
    private readonly AplicarPenalizacionValidator _validator = new();

    [Fact]
    public void Validar_ComandoCompleto_NoTieneErrores()
    {
        var result = _validator.Validate(
            new AplicarPenalizacionCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                10,
                "Conducta antideportiva",
                Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_PuntosNoPositivos_TieneError()
    {
        var result = _validator.Validate(
            new AplicarPenalizacionCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "Motivo",
                Guid.NewGuid()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AplicarPenalizacionCommand.Puntos));
    }

    [Fact]
    public void Validar_MotivoVacio_TieneError()
    {
        var result = _validator.Validate(
            new AplicarPenalizacionCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                5,
                "",
                Guid.NewGuid()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AplicarPenalizacionCommand.Motivo));
    }
}
