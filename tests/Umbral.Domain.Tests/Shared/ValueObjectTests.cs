using FluentAssertions;
using Umbral.Domain.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Shared;

/// <summary>
/// Tests de la clase base <c>ValueObject</c> (igualdad estructural).
/// Usa <see cref="Puntaje"/> como VO concreto.
/// </summary>
public sealed class ValueObjectTests
{
    [Fact]
    public void Equals_CuandoMismosComponentes_SonIguales()
    {
        var a = Puntaje.Crear(10);
        var b = Puntaje.Crear(10);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equals_CuandoComponentesDistintos_NoSonIguales()
    {
        var a = Puntaje.Crear(10);
        var b = Puntaje.Crear(20);

        a.Equals(b).Should().BeFalse();
        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equals_CuandoNull_DevuelveFalse()
    {
        var a = Puntaje.Crear(10);

        a.Equals(null).Should().BeFalse();
        (a == null).Should().BeFalse();
        (null! == a).Should().BeFalse();
    }

    [Fact]
    public void Equals_CuandoOtroTipo_DevuelveFalse()
    {
        var a = Puntaje.Crear(10);

        a.Equals("no soy un Puntaje").Should().BeFalse();
    }

    [Fact]
    public void Equals_CuandoAmbosNull_OperadorDevuelveTrue()
    {
        Puntaje? a = null;
        Puntaje? b = null;

        (a == b).Should().BeTrue();
    }
}
