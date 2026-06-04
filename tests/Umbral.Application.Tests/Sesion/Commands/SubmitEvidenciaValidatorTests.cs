using FluentAssertions;
using Umbral.Application.Sesion.Commands.SubmitEvidencia;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class SubmitEvidenciaValidatorTests
{
    private readonly SubmitEvidenciaValidator _validator = new();

    [Fact]
    public void Validar_ComandoCompleto_NoTieneErrores()
    {
        // Arrange
        var command = new SubmitEvidenciaCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "QR-ALFA-001");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_ParticipanteIdVacio_TieneError()
    {
        // Arrange
        var command = new SubmitEvidenciaCommand(
            Guid.NewGuid(),
            Guid.Empty,
            "QR-ALFA-001");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SubmitEvidenciaCommand.ParticipanteId));
    }

    [Fact]
    public void Validar_CodigoQrVacio_TieneError()
    {
        // Arrange
        var command = new SubmitEvidenciaCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            string.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SubmitEvidenciaCommand.CodigoQr));
    }
}
