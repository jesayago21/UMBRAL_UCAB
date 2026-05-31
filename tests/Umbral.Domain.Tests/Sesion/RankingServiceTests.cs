using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

public sealed class RankingServiceTests
{
    [Fact]
    public void Calcular_CuandoEquiposConDistintoPuntaje_OrdenaDescendente()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConEquipo("Alpha").ConEquipo("Beta").Build();
        var alpha = sesion.Equipos.First(e => e.Nombre.Valor == "Alpha");
        var beta  = sesion.Equipos.First(e => e.Nombre.Valor == "Beta");
        alpha.SumarPuntaje(50);
        beta.SumarPuntaje(100);

        // Act
        var ranking = RankingService.Calcular(sesion.Equipos);

        // Assert
        ranking.Should().HaveCount(2);
        ranking[0].NombreEquipo.Should().Be("Beta");
        ranking[0].PuntajeTotal.Should().Be(100);
        ranking[1].NombreEquipo.Should().Be("Alpha");
    }

    [Fact]
    public void Calcular_CuandoEmpateEnPuntaje_OrdenaPorNombre()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConEquipo("Gamma").ConEquipo("Alpha").Build();
        sesion.Equipos.First(e => e.Nombre.Valor == "Gamma").SumarPuntaje(50);
        sesion.Equipos.First(e => e.Nombre.Valor == "Alpha").SumarPuntaje(50);

        // Act
        var ranking = RankingService.Calcular(sesion.Equipos);

        // Assert
        ranking[0].NombreEquipo.Should().Be("Alpha");
        ranking[1].NombreEquipo.Should().Be("Gamma");
    }
}
