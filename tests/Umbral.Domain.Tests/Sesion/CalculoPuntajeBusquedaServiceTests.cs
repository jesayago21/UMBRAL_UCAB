using FluentAssertions;
using Umbral.Domain.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

public sealed class CalculoPuntajeBusquedaServiceTests
{
    [Fact]
    public void Calcular_CuandoEsGanador_Retorna100Puntos()
    {
        // Act
        var puntos = CalculoPuntajeBusquedaService.Calcular(esGanador: true);

        // Assert
        puntos.Valor.Should().Be(100);
    }

    [Fact]
    public void Calcular_CuandoNoEsGanador_RetornaCero()
    {
        // Act
        var puntos = CalculoPuntajeBusquedaService.Calcular(esGanador: false);

        // Assert
        puntos.Valor.Should().Be(0);
    }
}
