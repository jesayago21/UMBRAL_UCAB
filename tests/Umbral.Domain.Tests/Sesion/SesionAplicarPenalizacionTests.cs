using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Iteración 4 — AplicarPenalizacion (HU-16).
/// Fuente: umbral-backend-spec.md §2.4, umbral-quality-spec.md §4.1.
///
/// Reglas:
/// RB-16-01: solo en estado Activa.
/// RB-16-02: participante debe pertenecer a la sesión.
/// RB-16-03: emite PenalizacionAplicada; puntaje no baja de cero.
/// RB-16-04: Penalizacion.Puntos > 0; Motivo no vacío.
/// </summary>
public sealed class SesionAplicarPenalizacionTests
{
    private static Penalizacion PenalizacionValida(int puntos = 10) =>
        new(puntos, "Trampa detectada", new UsuarioId(Guid.NewGuid()));

    // ── Happy path ─────────────────────────────────────────────────

    [Fact]
    public void AplicarPenalizacion_CuandoSesionActiva_RestarPuntaje()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Alpha").Build();
        var participante      = sesion.Participantes.First();
        var penalizacion = new Penalizacion(10, "Trampa detectada",
            new UsuarioId(Guid.NewGuid()));

        participante.SumarPuntaje(50);

        // Act
        sesion.AplicarPenalizacion(participante.ParticipanteId, penalizacion);

        // Assert
        participante.PuntajeTotal.Valor.Should().Be(40);
    }

    [Fact]
    public void AplicarPenalizacion_CuandoSesionActiva_EmitePenalizacionAplicadaEvent()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.AplicarPenalizacion(participante.ParticipanteId, PenalizacionValida());

        // Assert
        sesion.DomainEvents.Should().ContainSingle(e => e is PenalizacionAplicada);
    }

    [Fact]
    public void AplicarPenalizacion_CuandoSesionActiva_EventoContieneDatosCorrectos()
    {
        // Arrange
        var operadorId   = new UsuarioId(Guid.NewGuid());
        var penalizacion = new Penalizacion(15, "Pista saltada", operadorId);

        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.AplicarPenalizacion(participante.ParticipanteId, penalizacion);

        // Assert
        var evt = sesion.DomainEvents.OfType<PenalizacionAplicada>().Single();
        evt.SesionId.Should().Be(sesion.SesionId);
        evt.ParticipanteId.Should().Be(participante.ParticipanteId);
        evt.Puntos.Should().Be(15);
        evt.Motivo.Should().Be("Pista saltada");
        evt.OperadorId.Should().Be(operadorId);
    }

    [Fact]
    public void AplicarPenalizacion_CuandoSesionActiva_RegistraEventoEnHistorial()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.AplicarPenalizacion(participante.ParticipanteId, PenalizacionValida());

        // Assert
        sesion.HistorialEventos.Should()
            .Contain(e => e.Tipo == "PenalizacionAplicada");
    }

    [Fact]
    public void AplicarPenalizacion_CuandoPuntajeMenorQuePenalizacion_ResultaCero()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();
        participante.SumarPuntaje(5);

        // Act
        sesion.AplicarPenalizacion(participante.ParticipanteId, PenalizacionValida(100));

        // Assert
        participante.PuntajeTotal.Valor.Should().Be(0);
    }

    [Fact]
    public void AplicarPenalizacion_CuandoParticipanteConPuntajeCero_PermaneceCero()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.AplicarPenalizacion(participante.ParticipanteId, PenalizacionValida(50));

        // Assert
        participante.PuntajeTotal.Valor.Should().Be(0);
    }

    [Fact]
    public void AplicarPenalizacion_VariasPenalizaciones_AcumulanCorrectamente()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();
        participante.SumarPuntaje(100);

        // Act
        sesion.AplicarPenalizacion(participante.ParticipanteId, PenalizacionValida(20));
        sesion.AplicarPenalizacion(participante.ParticipanteId, PenalizacionValida(15));

        // Assert
        participante.PuntajeTotal.Valor.Should().Be(65);
    }

    // ── Guard: estado de sesion ────────────────────────────────────

    [Theory]
    [InlineData(EstadoSesion.Programada)]
    [InlineData(EstadoSesion.EnPreparacion)]
    [InlineData(EstadoSesion.Pausada)]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void AplicarPenalizacion_CuandoSesionNoActiva_LanzaDomainException(
        EstadoSesion estadoInvalido)
    {
        // Arrange
        var sesion   = SesionBuilder.BusquedaTesoro().ConEstado(estadoInvalido).Build();
        var participanteId = ParticipanteId.Nuevo();

        // Act
        var act = () => sesion.AplicarPenalizacion(participanteId, PenalizacionValida());

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── Guard: participante inexistente ──────────────────────────────────

    [Fact]
    public void AplicarPenalizacion_CuandoParticipanteNoPerteneceSesion_LanzaDomainException()
    {
        // Arrange
        var sesion         = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participanteAjeno    = ParticipanteId.Nuevo();

        // Act
        var act = () => sesion.AplicarPenalizacion(participanteAjeno, PenalizacionValida());

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── Guard: Penalizacion mal formada ────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AplicarPenalizacion_CuandoPuntosInvalidos_LanzaDomainException(int puntos)
    {
        // Arrange — la excepción se lanza al construir Penalizacion, antes de llegar a Sesion
        var act = () => new Penalizacion(puntos, "Motivo válido",
            new UsuarioId(Guid.NewGuid()));

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AplicarPenalizacion_CuandoMotivoVacio_LanzaDomainException(string motivo)
    {
        // Arrange — la excepción se lanza al construir Penalizacion
        var act = () => new Penalizacion(10, motivo,
            new UsuarioId(Guid.NewGuid()));

        // Assert
        act.Should().Throw<DomainException>();
    }
}
