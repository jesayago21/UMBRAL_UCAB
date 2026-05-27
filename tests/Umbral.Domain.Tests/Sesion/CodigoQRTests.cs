using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Tests del Value Object CodigoQR (domain-model.md, umbral-product-spec RF-20).
/// </summary>
public sealed class CodigoQRTests
{
    [Fact]
    public void Crear_CuandoValorValido_AlmacenaTrim()
    {
        // Act
        var qr = CodigoQR.Crear("  QR-TEST-001  ");

        // Assert
        qr.Valor.Should().Be("QR-TEST-001");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_CuandoValorVacio_LanzaDomainException(string valor)
    {
        // Act
        var act = () => CodigoQR.Crear(valor);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CoincideCon_CuandoMismoCodigoCaseInsensitive_RetornaTrue()
    {
        // Arrange
        var qr = CodigoQR.Crear("QR-ARBOL-001");

        // Act & Assert
        qr.CoincideCon("qr-arbol-001").Should().BeTrue();
        qr.CoincideCon("QR-ARBOL-001").Should().BeTrue();
    }

    [Fact]
    public void CoincideCon_CuandoCodigoDistinto_RetornaFalse()
    {
        // Arrange
        var qr = CodigoQR.Crear("QR-ARBOL-001");

        // Act & Assert
        qr.CoincideCon("QR-FUENTE-002").Should().BeFalse();
    }

    [Fact]
    public void Igualdad_CuandoMismoValorCaseInsensitive_SonIguales()
    {
        // Arrange
        var a = CodigoQR.Crear("QR-TEST");
        var b = CodigoQR.Crear("qr-test");

        // Assert
        a.Should().Be(b);
    }
}
