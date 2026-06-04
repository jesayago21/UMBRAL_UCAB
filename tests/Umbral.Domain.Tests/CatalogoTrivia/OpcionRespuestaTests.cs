using FluentAssertions;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.CatalogoTrivia;

/// <summary>
/// Tests del VO OpcionRespuesta (banco de preguntas de Trivia, HU-24).
/// </summary>
public sealed class OpcionRespuestaTests
{
    [Fact]
    public void Crear_ConTextoValido_RetornaOpcion()
    {
        // Act
        var opcion = OpcionRespuesta.Crear("París", esCorrecta: true);

        // Assert
        opcion.Texto.Should().Be("París");
        opcion.EsCorrecta.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConTextoVacio_LanzaDomainException(string texto)
    {
        // Act
        var act = () => OpcionRespuesta.Crear(texto, esCorrecta: false);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Crear_RecortaEspaciosDelTexto()
    {
        // Act
        var opcion = OpcionRespuesta.Crear("  Madrid  ", esCorrecta: false);

        // Assert
        opcion.Texto.Should().Be("Madrid");
    }

    [Fact]
    public void Equals_DosOpcionesConMismoTextoYEstado_SonIguales()
    {
        // Arrange
        var a = OpcionRespuesta.Crear("París", esCorrecta: true);
        var b = OpcionRespuesta.Crear("París", esCorrecta: true);

        // Assert
        a.Should().Be(b);
    }
}
