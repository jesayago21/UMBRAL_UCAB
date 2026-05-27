using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Validacion;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Tests del Domain Service ValidacionEvidenciaService (HU-18, RB-06, RB-19, RB-22).
/// </summary>
public sealed class ValidacionEvidenciaServiceTests
{
    private const string QrEtapa1 = "QR-ARBOL-001";
    private const string QrEtapa2 = "QR-FUENTE-002";

    [Fact]
    public void Validar_CuandoSesionActivaYQrCoincide_RetornaValida()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var qr     = CodigoQR.Crear(QrEtapa1);

        // Act
        var resultado = ValidacionEvidenciaService.Validar(sesion, qr);

        // Assert
        resultado.Should().Be(ResultadoValidacion.Valida);
    }

    [Fact]
    public void Validar_CuandoQrNoCoincideConEtapaActiva_RetornaInvalida()
    {
        // Arrange — etapa activa es la 1 (QR-ARBOL-001)
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var qr     = CodigoQR.Crear(QrEtapa2);

        // Act
        var resultado = ValidacionEvidenciaService.Validar(sesion, qr);

        // Assert
        resultado.Should().Be(ResultadoValidacion.Invalida);
    }

    [Theory]
    [InlineData(EstadoSesion.Pausada)]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    [InlineData(EstadoSesion.EnPreparacion)]
    [InlineData(EstadoSesion.Programada)]
    public void Validar_CuandoSesionNoActiva_RetornaRechazada(EstadoSesion estado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(estado).Build();
        var qr     = CodigoQR.Crear(QrEtapa1);

        // Act
        var resultado = ValidacionEvidenciaService.Validar(sesion, qr);

        // Assert
        resultado.Should().Be(ResultadoValidacion.Rechazada);
    }
}
