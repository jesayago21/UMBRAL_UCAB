using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Tests del Value Object Puntaje (umbral-quality-spec.md §4.2).
/// </summary>
public sealed class PuntajeTests
{
    // ── Crear ──────────────────────────────────────────────────────

    [Fact]
    public void Crear_CuandoValorNegativo_LanzaDomainException()
    {
        // Arrange / Act
        var act = () => Puntaje.Crear(-1);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Crear_CuandoValorCero_EsValido()
    {
        // Arrange / Act
        var puntaje = Puntaje.Crear(0);

        // Assert
        puntaje.Valor.Should().Be(0);
    }

    [Fact]
    public void Crear_CuandoValorPositivo_AlmacenaValor()
    {
        // Arrange / Act
        var puntaje = Puntaje.Crear(100);

        // Assert
        puntaje.Valor.Should().Be(100);
    }

    [Fact]
    public void Zero_RetornaPuntajeCero()
    {
        // Arrange / Act
        var puntaje = Puntaje.Zero();

        // Assert
        puntaje.Valor.Should().Be(0);
    }

    // ── Sumar ──────────────────────────────────────────────────────

    [Fact]
    public void Sumar_RetornaValorCorrecto()
    {
        // Arrange
        var puntaje = Puntaje.Crear(30);

        // Act
        var resultado = puntaje.Sumar(20);

        // Assert
        resultado.Valor.Should().Be(50);
    }

    [Fact]
    public void Sumar_NoMutaInstanciaOriginal()
    {
        // Arrange
        var puntaje = Puntaje.Crear(30);

        // Act
        _ = puntaje.Sumar(20);

        // Assert
        puntaje.Valor.Should().Be(30);
    }

    [Fact]
    public void Sumar_VariasVeces_AcumulaCorrectamente()
    {
        // Arrange
        var puntaje = Puntaje.Zero();

        // Act
        var resultado = puntaje.Sumar(10).Sumar(20).Sumar(30);

        // Assert
        resultado.Valor.Should().Be(60);
    }

    // ── Restar ─────────────────────────────────────────────────────

    [Fact]
    public void Restar_CuandoResultadoSeriaNegatvo_RetornaCero()
    {
        // Arrange
        var puntaje = Puntaje.Crear(10);

        // Act
        var resultado = puntaje.Restar(50);

        // Assert
        resultado.Valor.Should().Be(0);
    }

    [Fact]
    public void Restar_CuandoCantidadExacta_RetornaCero()
    {
        // Arrange
        var puntaje = Puntaje.Crear(25);

        // Act
        var resultado = puntaje.Restar(25);

        // Assert
        resultado.Valor.Should().Be(0);
    }

    [Fact]
    public void Restar_CuandoCantidadMenor_RestaCorrectamente()
    {
        // Arrange
        var puntaje = Puntaje.Crear(100);

        // Act
        var resultado = puntaje.Restar(30);

        // Assert
        resultado.Valor.Should().Be(70);
    }

    [Fact]
    public void Restar_NoMutaInstanciaOriginal()
    {
        // Arrange
        var puntaje = Puntaje.Crear(100);

        // Act
        _ = puntaje.Restar(30);

        // Assert
        puntaje.Valor.Should().Be(100);
    }

    // ── Igualdad (ValueObject) ──────────────────────────────────────

    [Fact]
    public void Igualdad_CuandoMismoValor_SonIguales()
    {
        // Arrange
        var a = Puntaje.Crear(50);
        var b = Puntaje.Crear(50);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Igualdad_CuandoDistintoValor_NoSonIguales()
    {
        // Arrange
        var a = Puntaje.Crear(50);
        var b = Puntaje.Crear(60);

        // Assert
        a.Should().NotBe(b);
    }
}
