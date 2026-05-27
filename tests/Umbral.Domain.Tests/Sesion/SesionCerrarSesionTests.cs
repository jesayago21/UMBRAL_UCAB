using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Iteración 7 — Finalizar / Cancelar sesión + ranking final (HU-23).
/// </summary>
public sealed class SesionCerrarSesionTests
{
    // ── Finalizar ──────────────────────────────────────────────────

    [Fact]
    public void Finalizar_CuandoEstaActiva_CambiaEstadoYRegistraFecha()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act
        sesion.Finalizar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
        sesion.FinalizadaEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Finalizar_CuandoEstaActiva_EmiteSesionFinalizada()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act
        sesion.Finalizar();

        // Assert
        sesion.DomainEvents.Should().ContainSingle(e => e is SesionFinalizada);
        sesion.HistorialEventos.Should().Contain(e => e.Tipo == "SesionFinalizada");
    }

    [Fact]
    public void Finalizar_CuandoEstaPausada_CambiaEstadoAFinalizada()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(EstadoSesion.Pausada).Build();

        // Act
        sesion.Finalizar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
    }

    [Theory]
    [InlineData(EstadoSesion.Programada)]
    [InlineData(EstadoSesion.EnPreparacion)]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void Finalizar_CuandoEstadoNoPermitido_LanzaDomainException(EstadoSesion estado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(estado).Build();

        // Act
        var act = () => sesion.Finalizar();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── Cancelar ─────────────────────────────────────────────────

    [Theory]
    [InlineData(EstadoSesion.Activa)]
    [InlineData(EstadoSesion.Pausada)]
    [InlineData(EstadoSesion.EnPreparacion)]
    [InlineData(EstadoSesion.Programada)]
    public void Cancelar_CuandoEstadoNoTerminal_CambiaEstadoACancelada(EstadoSesion estado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(estado).Build();

        // Act
        sesion.Cancelar("Motivo de prueba");

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Cancelada);
        sesion.FinalizadaEn.Should().NotBeNull();
    }

    [Fact]
    public void Cancelar_CuandoEstaActiva_EmiteSesionCanceladaConMotivo()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act
        sesion.Cancelar("  Clima adverso  ");

        // Assert
        var evt = sesion.DomainEvents.OfType<SesionCancelada>().Single();
        evt.Motivo.Should().Be("Clima adverso");
        sesion.HistorialEventos.Should().Contain(e =>
            e.Tipo == "SesionCancelada" && e.Payload.Contains("Clima adverso"));
    }

    [Theory]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void Cancelar_CuandoEstadoTerminal_LanzaDomainException(EstadoSesion estado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(estado).Build();

        // Act
        var act = () => sesion.Cancelar("Motivo");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancelar_CuandoMotivoVacio_LanzaDomainException(string motivo)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act
        var act = () => sesion.Cancelar(motivo);

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── Ranking final (HU-23) ─────────────────────────────────────

    [Fact]
    public void ObtenerRankingFinal_CuandoEstaActiva_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act
        var act = () => sesion.ObtenerRankingFinal();

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ObtenerRankingFinal_CuandoEstaFinalizada_RetornaEquiposOrdenados()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConEquipo("Alpha").ConEquipo("Beta").Build();
        sesion.Equipos.First(e => e.Nombre.Valor == "Beta").SumarPuntaje(80);
        sesion.Equipos.First(e => e.Nombre.Valor == "Alpha").SumarPuntaje(120);
        sesion.Finalizar();
        sesion.ClearDomainEvents();

        // Act
        var ranking = sesion.ObtenerRankingFinal();

        // Assert
        ranking.Should().HaveCount(2);
        ranking[0].NombreEquipo.Should().Be("Alpha");
        ranking[0].PuntajeTotal.Should().Be(120);
        ranking[0].Posicion.Should().Be(1);
    }

    [Fact]
    public void ObtenerRankingFinal_CuandoEstaCancelada_RetornaRanking()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConEquipo("Alpha").Build();
        sesion.Equipos.First().SumarPuntaje(50);
        sesion.Cancelar("Cancelada");

        // Act
        var ranking = sesion.ObtenerRankingFinal();

        // Assert
        ranking.Should().ContainSingle();
        ranking[0].PuntajeTotal.Should().Be(50);
    }
}
