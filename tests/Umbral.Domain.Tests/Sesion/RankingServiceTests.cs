using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

public sealed class RankingServiceTests
{
    [Fact]
    public void Calcular_CuandoParticipantesConDistintoPuntaje_OrdenaDescendente()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Alpha").ConParticipante("Beta").Build();
        var alpha = sesion.Participantes.First(e => e.Nombre.Valor == "Alpha");
        var beta  = sesion.Participantes.First(e => e.Nombre.Valor == "Beta");
        alpha.SumarPuntaje(50);
        beta.SumarPuntaje(100);

        // Act
        var ranking = RankingService.Calcular(sesion.Participantes);

        // Assert
        ranking.Should().HaveCount(2);
        ranking[0].NombreParticipante.Should().Be("Beta");
        ranking[0].PuntajeTotal.Should().Be(100);
        ranking[1].NombreParticipante.Should().Be("Alpha");
    }

    [Fact]
    public void Calcular_CuandoEmpateEnPuntaje_OrdenaPorNombre()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Gamma").ConParticipante("Alpha").Build();
        sesion.Participantes.First(e => e.Nombre.Valor == "Gamma").SumarPuntaje(50);
        sesion.Participantes.First(e => e.Nombre.Valor == "Alpha").SumarPuntaje(50);

        // Act
        var ranking = RankingService.Calcular(sesion.Participantes);

        // Assert
        ranking[0].NombreParticipante.Should().Be("Alpha");
        ranking[1].NombreParticipante.Should().Be("Gamma");
    }
}
