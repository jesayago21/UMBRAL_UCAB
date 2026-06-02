using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using SesionAR = Umbral.Domain.Sesion.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Iteración 3 — ciclo de vida de la sesión (HU-14 + HU-15).
/// Fuente: umbral-backend-spec.md §2.4, umbral-quality-spec.md §4.1.
///
/// HU-14: IniciarSesion  (Programada → EnPreparacion → Activa)
/// HU-15: PausarReanudar (Activa ⇄ Pausada)
/// </summary>
public sealed class SesionCicloVidaTests
{
    // ═══════════════════════════════════════════════════════════════
    // AbrirParaRegistro  Programada → EnPreparacion
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void AbrirParaRegistro_CuandoEstaProgramada_CambiaEstadoAEnPreparacion()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.Programada).Build();

        // Act
        sesion.AbrirParaRegistro();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.EnPreparacion);
    }

    [Fact]
    public void AbrirParaRegistro_CuandoEstaProgramada_RegistraEventoEnHistorial()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.Programada).Build();

        // Act
        sesion.AbrirParaRegistro();

        // Assert
        sesion.HistorialEventos.Should()
            .ContainSingle(e => e.Tipo == "SesionAbiertaParaRegistro");
    }

    [Theory]
    [InlineData(EstadoSesion.EnPreparacion)]
    [InlineData(EstadoSesion.Activa)]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void AbrirParaRegistro_CuandoNoEstaProgramada_LanzaDomainException(
        EstadoSesion estadoInvalido)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(estadoInvalido).Build();

        // Act
        var act = () => sesion.AbrirParaRegistro();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // Iniciar  EnPreparacion → Activa
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Iniciar_CuandoEstaEnPreparacion_CambiaEstadoAActiva()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .ConParticipante("Alpha").Build();

        // Act
        sesion.Iniciar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Activa);
        sesion.IniciadaEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Iniciar_CuandoEstaEnPreparacion_EmiteSesionIniciadaEvent()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .ConParticipante("Alpha").Build();

        // Act
        sesion.Iniciar();

        // Assert
        sesion.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SesionIniciada>();
    }

    [Fact]
    public void Iniciar_CuandoEstaEnPreparacion_EventoContieneSesionIdYTipo()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .ConParticipante("Alpha").Build();

        // Act
        sesion.Iniciar();

        // Assert
        var evt = sesion.DomainEvents.OfType<SesionIniciada>().Single();
        evt.SesionId.Should().Be(sesion.SesionId);
        evt.TipoSesion.Should().Be(TipoSesion.Mision);
    }

    [Fact]
    public void Iniciar_CuandoEstaEnPreparacion_RegistraEventoEnHistorial()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .ConParticipante("Alpha").Build();

        // Act
        sesion.Iniciar();

        // Assert
        sesion.HistorialEventos.Should()
            .Contain(e => e.Tipo == "SesionIniciada");
    }

    [Fact]
    public void Iniciar_SinParticipantesRegistrados_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .SinParticipantes().Build();

        // Act
        var act = () => sesion.Iniciar();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*al menos un participante*");
    }

    [Theory]
    [InlineData(EstadoSesion.Programada)]
    [InlineData(EstadoSesion.Activa)]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void Iniciar_CuandoEstadoNoEsEnPreparacion_LanzaDomainException(
        EstadoSesion estadoInvalido)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(estadoInvalido).Build();

        // Act
        var act = () => sesion.Iniciar();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // Pausar  Activa → Pausada
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Pausar_CuandoEstaActiva_CambiaEstadoAPausada()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act
        sesion.Pausar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Pausada);
    }

    [Fact]
    public void Pausar_CuandoEstaActiva_EmiteSesionPausadaEvent()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act
        sesion.Pausar();

        // Assert
        sesion.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SesionPausada>();
    }

    [Fact]
    public void Pausar_CuandoEstaActiva_RegistraEventoEnHistorial()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act
        sesion.Pausar();

        // Assert
        sesion.HistorialEventos.Should()
            .Contain(e => e.Tipo == "SesionPausada");
    }

    [Theory]
    [InlineData(EstadoSesion.Programada)]
    [InlineData(EstadoSesion.EnPreparacion)]
    [InlineData(EstadoSesion.Pausada)]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void Pausar_CuandoNoEstaActiva_LanzaDomainException(EstadoSesion estado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(estado).Build();

        // Act
        var act = () => sesion.Pausar();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // Reanudar  Pausada → Activa
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Reanudar_CuandoEstaPausada_CambiaEstadoAActiva()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(EstadoSesion.Pausada).Build();

        // Act
        sesion.Reanudar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Activa);
    }

    [Fact]
    public void Reanudar_CuandoEstaPausada_EmiteSesionReanudadaEvent()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(EstadoSesion.Pausada).Build();

        // Act
        sesion.Reanudar();

        // Assert
        sesion.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SesionReanudada>();
    }

    [Fact]
    public void Reanudar_CuandoEstaPausada_RegistraEventoEnHistorial()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(EstadoSesion.Pausada).Build();

        // Act
        sesion.Reanudar();

        // Assert
        sesion.HistorialEventos.Should()
            .Contain(e => e.Tipo == "SesionReanudada");
    }

    [Theory]
    [InlineData(EstadoSesion.Programada)]
    [InlineData(EstadoSesion.EnPreparacion)]
    [InlineData(EstadoSesion.Activa)]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void Reanudar_CuandoNoEstaPausada_LanzaDomainException(EstadoSesion estado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(estado).Build();

        // Act
        var act = () => sesion.Reanudar();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // Ciclo completo
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void CicloCompleto_AbrirRegistrarIniciarPausarReanudar_EstadosCorrectos()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.Programada).Build();

        // Act & Assert — cada transición es válida
        sesion.AbrirParaRegistro();
        sesion.Estado.Should().Be(EstadoSesion.EnPreparacion);

        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        sesion.Iniciar();
        sesion.Estado.Should().Be(EstadoSesion.Activa);

        sesion.Pausar();
        sesion.Estado.Should().Be(EstadoSesion.Pausada);

        sesion.Reanudar();
        sesion.Estado.Should().Be(EstadoSesion.Activa);
    }
}
