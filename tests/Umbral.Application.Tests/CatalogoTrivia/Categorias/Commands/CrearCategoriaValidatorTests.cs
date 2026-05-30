using FluentAssertions;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.CrearCategoria;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Categorias.Commands;

public sealed class CrearCategoriaValidatorTests
{
    private readonly CrearCategoriaValidator _validator = new();

    [Fact]
    public void Validar_NombreValido_NoTieneErrores()
    {
        var result = _validator.Validate(new CrearCategoriaCommand("Historia"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_NombreVacio_TieneError()
    {
        var result = _validator.Validate(new CrearCategoriaCommand(""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearCategoriaCommand.Nombre));
    }
}
