using FluentValidation.TestHelper;
using Umbral.Application.Sesion.Commands.CrearSesionTrivia;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class CrearSesionTriviaValidatorTests
{
    private readonly CrearSesionTriviaValidator _validator = new();

    [Fact]
    public void Validate_ConDatosValidos_NoTieneErrores()
    {
        var command = new CrearSesionTriviaCommand(
            [Guid.NewGuid()],
            Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_SinCategorias_TieneError()
    {
        var command = new CrearSesionTriviaCommand([], Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CategoriaIds);
    }
}
