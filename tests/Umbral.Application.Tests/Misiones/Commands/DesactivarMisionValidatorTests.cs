using FluentAssertions;
using Umbral.Application.Misiones.Commands.DesactivarMision;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class DesactivarMisionValidatorTests
{
    private readonly DesactivarMisionValidator _sut = new();

    [Fact]
    public void Validate_ConMisionIdValido_SinErrores()
    {
        var result = _sut.Validate(new DesactivarMisionCommand(Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_MisionIdVacio_Error()
    {
        var result = _sut.Validate(new DesactivarMisionCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
    }
}
