using FluentAssertions;
using Umbral.Application.Sesion.Commands.CancelarSesion;
using Umbral.Application.Sesion.Commands.FinalizarSesion;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class CierreSesionValidatorTests
{
    [Fact]
    public void FinalizarSesion_SesionIdVacio_TieneError()
    {
        // Act
        var result = new FinalizarSesionValidator().Validate(new FinalizarSesionCommand(Guid.Empty));

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CancelarSesion_MotivoVacio_TieneError()
    {
        // Act
        var result = new CancelarSesionValidator().Validate(
            new CancelarSesionCommand(Guid.NewGuid(), string.Empty));

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
