using FluentAssertions;
using Umbral.Application.Sesion.Commands.IniciarSesion;
using Umbral.Application.Sesion.Commands.PausarSesion;
using Umbral.Application.Sesion.Commands.ReanudarSesion;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class CicloVidaSesionValidatorTests
{
    [Fact]
    public void IniciarSesion_SesionIdVacio_TieneError()
    {
        var result = new IniciarSesionValidator().Validate(new IniciarSesionCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PausarSesion_SesionIdVacio_TieneError()
    {
        var result = new PausarSesionValidator().Validate(new PausarSesionCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ReanudarSesion_SesionIdVacio_TieneError()
    {
        var result = new ReanudarSesionValidator().Validate(new ReanudarSesionCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
    }
}
