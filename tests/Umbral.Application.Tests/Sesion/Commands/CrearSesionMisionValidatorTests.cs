using FluentAssertions;
using Umbral.Application.Sesion.Commands.CrearSesionMision;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class CrearSesionMisionValidatorTests
{
    private readonly CrearSesionMisionValidator _sut = new();

    [Fact]
    public void Validate_ConDatosValidos_SinErrores()
    {
        var result = _sut.Validate(new CrearSesionMisionCommand(Guid.NewGuid(), Guid.NewGuid(), "Grupo mañana"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MisionIdVacio_Error()
    {
        var result = _sut.Validate(new CrearSesionMisionCommand(Guid.Empty, Guid.NewGuid(), "Grupo mañana"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearSesionMisionCommand.MisionId));
    }

    [Fact]
    public void Validate_OperadorIdVacio_Error()
    {
        var result = _sut.Validate(new CrearSesionMisionCommand(Guid.NewGuid(), Guid.Empty, "Grupo mañana"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearSesionMisionCommand.OperadorId));
    }
}
