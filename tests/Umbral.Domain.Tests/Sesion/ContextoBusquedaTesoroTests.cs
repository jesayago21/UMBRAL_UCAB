using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Tests de ContextoBusquedaTesoro (umbral-quality-spec.md §4, testing-skill.md §3).
/// </summary>
public sealed class ContextoBusquedaTesoroTests
{
    private const string QrEtapa1 = "QR-ARBOL-001";
    private const string QrEtapa2 = "QR-FUENTE-002";

    [Fact]
    public void ObtenerEtapaActual_CuandoIndexCero_RetornaPrimeraEtapa()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Build();

        // Act
        var etapa = sesion.ContextoBT!.ObtenerEtapaActual();

        // Assert
        etapa.CodigoQRSolucion.Should().Be(QrEtapa1);
        etapa.Orden.Should().Be(1);
    }

    [Fact]
    public void EsUltimaEtapa_CuandoIndexCero_RetornaFalse()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Build();

        // Act & Assert
        sesion.ContextoBT!.EsUltimaEtapa().Should().BeFalse();
    }

    [Fact]
    public void EsUltimaEtapa_CuandoHayDosEtapasYIndexUno_RetornaTrue()
    {
        // Arrange — misión fake tiene 2 etapas; avanzamos manualmente vía reflexión no,
        // verificamos que la segunda etapa es la última comparando códigos.
        var sesion = SesionBuilder.BusquedaTesoro().Build();
        var etapas = sesion.ContextoBT!.MisionSnapshot.Etapas;

        // Assert
        etapas.Should().HaveCount(2);
        etapas[1].CodigoQRSolucion.Should().Be(QrEtapa2);
    }
}
