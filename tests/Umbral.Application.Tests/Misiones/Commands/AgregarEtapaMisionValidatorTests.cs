using FluentValidation.TestHelper;
using Umbral.Application.Misiones.Commands.AgregarEtapaMision;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class AgregarEtapaMisionValidatorTests
{
    private readonly AgregarEtapaMisionValidator _validator = new();

    [Fact]
    public void Validate_EtapaBtValida_NoTieneErrores()
    {
        var cmd = new AgregarEtapaMisionCommand(
            Guid.NewGuid(),
            "BusquedaTesoro",
            "Hall",
            "QR-001",
            null,
            null,
            null,
            null,
            null);

        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EtapaTriviaSinCategorias_TieneError()
    {
        var cmd = new AgregarEtapaMisionCommand(
            Guid.NewGuid(),
            "Trivia",
            null,
            null,
            [],
            null,
            null,
            null,
            null);

        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.CategoriaIds);
    }

    [Fact]
    public void Validate_UbicacionIncompleta_TieneError()
    {
        var cmd = new AgregarEtapaMisionCommand(
            Guid.NewGuid(),
            "BusquedaTesoro",
            "Hall",
            "QR-001",
            null,
            null,
            10.5,
            null,
            100);

        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x);
    }
}
