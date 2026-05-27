using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;

using SesionAggregate = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Tests de la Iteración 1 — caso de uso: IniciarSesion.
///
/// Reglas cubiertas:
///   R1 — No se puede iniciar sin equipos registrados.
///   R2 — La transición Iniciar() solo es válida desde Preparacion.
/// </summary>
public class SesionIniciarTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static SesionAggregate CrearSesionVacia() =>
        SesionAggregate.Crear(SesionId.Nuevo(), TipoSesion.BusquedaTesoro);

    private static EquipoSesion CrearEquipo(string nombre = "Equipo Alfa") =>
        EquipoSesion.Crear(EquipoSesionId.Nuevo(), nombre);

    private static readonly DateTime FijaAhora =
        new(2026, 5, 26, 21, 0, 0, DateTimeKind.Utc);

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Iniciar_ConUnEquipoRegistrado_CambiaEstadoAActiva()
    {
        // Arrange
        var sesion = CrearSesionVacia();
        sesion.RegistrarEquipo(CrearEquipo());

        // Act
        sesion.Iniciar(FijaAhora);

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Activa);
    }

    [Fact]
    public void Iniciar_ConVariosEquipos_CambiaEstadoAActiva()
    {
        // Arrange
        var sesion = CrearSesionVacia();
        sesion.RegistrarEquipo(CrearEquipo("Equipo Alfa"));
        sesion.RegistrarEquipo(CrearEquipo("Equipo Beta"));
        sesion.RegistrarEquipo(CrearEquipo("Equipo Gamma"));

        // Act
        sesion.Iniciar(FijaAhora);

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Activa);
    }

    [Fact]
    public void Iniciar_RegistraTimestampDeInicio()
    {
        // Arrange
        var sesion = CrearSesionVacia();
        sesion.RegistrarEquipo(CrearEquipo());

        // Act
        sesion.Iniciar(FijaAhora);

        // Assert
        sesion.IniciadaEn.Should().Be(FijaAhora);
    }

    [Fact]
    public void Iniciar_EmiteDomainEventSesionIniciada()
    {
        // Arrange
        var id = SesionId.Nuevo();
        var sesion = SesionAggregate.Crear(id, TipoSesion.BusquedaTesoro);
        sesion.RegistrarEquipo(CrearEquipo());

        // Act
        sesion.Iniciar(FijaAhora);

        // Assert
        sesion.DomainEvents.Should().HaveCount(1);

        var evento = sesion.DomainEvents[0].Should()
            .BeOfType<SesionIniciada>().Subject;

        evento.SesionId.Should().Be(id);
        evento.OcurridoEn.Should().Be(FijaAhora);
    }

    // ── R1: sin equipos → DomainException ────────────────────────────────────

    [Fact]
    public void Iniciar_SinEquipos_LanzaDomainException()
    {
        // Arrange
        var sesion = CrearSesionVacia();

        // Act
        var act = () => sesion.Iniciar(FijaAhora);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*sin equipos*");
    }

    [Fact]
    public void Iniciar_SinEquipos_NoEmiteDomainEvent()
    {
        // Arrange
        var sesion = CrearSesionVacia();

        // Act
        try { sesion.Iniciar(FijaAhora); } catch { /* ignorar */ }

        // Assert
        sesion.DomainEvents.Should().BeEmpty();
    }

    // ── R2: estado inválido → DomainException ─────────────────────────────────

    [Fact]
    public void Iniciar_DesdeEstadoActiva_LanzaDomainException()
    {
        // Arrange — sesión ya iniciada
        var sesion = CrearSesionVacia();
        sesion.RegistrarEquipo(CrearEquipo());
        sesion.Iniciar(FijaAhora);

        // Act — intento de segundo inicio
        var act = () => sesion.Iniciar(FijaAhora.AddMinutes(1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*'Activa'*");
    }

    [Theory]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    [InlineData(EstadoSesion.Pausada)]
    public void Iniciar_DesdeEstadoTerminalOPausado_LanzaDomainException(
        EstadoSesion estadoInvalido)
    {
        // Arrange — forzar estado vía reflexión (solo en tests para simular
        // estados que aún no tienen métodos de transición en esta iteración)
        var sesion = CrearSesionVacia();
        sesion.RegistrarEquipo(CrearEquipo());

        var propEstado = typeof(SesionAggregate)
            .GetProperty(nameof(SesionAggregate.Estado))!;
        var setterEstado = propEstado.GetSetMethod(nonPublic: true)!;
        setterEstado.Invoke(sesion, [estadoInvalido]);

        // Act
        var act = () => sesion.Iniciar(FijaAhora);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"*'{estadoInvalido}'*");
    }
}
