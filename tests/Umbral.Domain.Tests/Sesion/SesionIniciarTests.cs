using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;

using SesionAggregate = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Pruebas de la Iteración 1 — caso de uso: IniciarSesion.
/// Alineadas a umbral-backend-spec.md y umbral-quality-spec.md.
///
/// Reglas cubiertas:
///   R1 — No se puede iniciar sin al menos un equipo registrado.
///   R2 — Solo es válido desde estado EnPreparacion.
///
/// Flujo real:  Crear() → AbrirParaRegistro() → RegistrarEquipo() → Iniciar()
/// </summary>
public class SesionIniciarTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Devuelve una sesión lista para iniciar (en EnPreparacion con un equipo).
    /// </summary>
    private static SesionAggregate SesionEnPreparacionConEquipo(string nombre = "Equipo Alfa")
    {
        var sesion = SesionAggregate.Crear(SesionId.Nuevo(), TipoSesion.BusquedaTesoro);
        sesion.AbrirParaRegistro();
        sesion.RegistrarEquipo(EquipoSesion.Crear(EquipoSesionId.Nuevo(), nombre));
        return sesion;
    }

    private static SesionAggregate SesionEnPreparacionSinEquipos()
    {
        var sesion = SesionAggregate.Crear(SesionId.Nuevo(), TipoSesion.BusquedaTesoro);
        sesion.AbrirParaRegistro();
        return sesion;
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Iniciar_CuandoEstaEnPreparacionConEquipo_CambiaEstadoAActiva()
    {
        // Arrange
        var sesion = SesionEnPreparacionConEquipo();

        // Act
        sesion.Iniciar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Activa);
    }

    [Fact]
    public void Iniciar_CuandoEstaEnPreparacion_RegistraTimestampDeInicio()
    {
        // Arrange
        var sesion = SesionEnPreparacionConEquipo();

        // Act
        sesion.Iniciar();

        // Assert
        sesion.IniciadaEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Iniciar_CuandoEstaEnPreparacion_EmiteSesionIniciada()
    {
        // Arrange
        var id     = SesionId.Nuevo();
        var sesion = SesionAggregate.Crear(id, TipoSesion.BusquedaTesoro);
        sesion.AbrirParaRegistro();
        sesion.RegistrarEquipo(EquipoSesion.Crear(EquipoSesionId.Nuevo(), "Equipo Alfa"));

        // Act
        sesion.Iniciar();

        // Assert
        sesion.DomainEvents.Should().ContainSingle(e => e is SesionIniciada);

        var evento = sesion.DomainEvents.OfType<SesionIniciada>().Single();
        evento.SesionId.Should().Be(id);
        evento.TipoSesion.Should().Be(TipoSesion.BusquedaTesoro);
        evento.OcurridoEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Iniciar_CuandoHayVariosEquipos_CambiaEstadoAActiva()
    {
        // Arrange
        var sesion = SesionAggregate.Crear(SesionId.Nuevo(), TipoSesion.BusquedaTesoro);
        sesion.AbrirParaRegistro();
        sesion.RegistrarEquipo(EquipoSesion.Crear(EquipoSesionId.Nuevo(), "Equipo Alfa"));
        sesion.RegistrarEquipo(EquipoSesion.Crear(EquipoSesionId.Nuevo(), "Equipo Beta"));
        sesion.RegistrarEquipo(EquipoSesion.Crear(EquipoSesionId.Nuevo(), "Equipo Gamma"));

        // Act
        sesion.Iniciar();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Activa);
    }

    // ── R1: sin equipos → DomainException ────────────────────────────────────

    [Fact]
    public void Iniciar_SinEquiposRegistrados_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionEnPreparacionSinEquipos();

        // Act
        var act = () => sesion.Iniciar();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*al menos un equipo*");
    }

    [Fact]
    public void Iniciar_SinEquiposRegistrados_NoEmiteNingunEvento()
    {
        // Arrange
        var sesion = SesionEnPreparacionSinEquipos();

        // Act
        try { sesion.Iniciar(); } catch { /* ignorar */ }

        // Assert
        sesion.DomainEvents.Should().BeEmpty();
    }

    // ── R2: estado inválido → DomainException ─────────────────────────────────

    [Theory]
    [InlineData(EstadoSesion.Programada)]
    [InlineData(EstadoSesion.Activa)]
    [InlineData(EstadoSesion.Pausada)]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void Iniciar_CuandoEstadoNoEsEnPreparacion_LanzaDomainException(
        EstadoSesion estadoInvalido)
    {
        // Arrange
        var sesion = SesionAggregate.Crear(SesionId.Nuevo(), TipoSesion.BusquedaTesoro);
        sesion.RegistrarEquipo(EquipoSesion.Crear(EquipoSesionId.Nuevo(), "Equipo Alfa"));

        var propEstado = typeof(SesionAggregate)
            .GetProperty(nameof(SesionAggregate.Estado))!;
        propEstado.GetSetMethod(nonPublic: true)!.Invoke(sesion, [estadoInvalido]);

        // Act
        var act = () => sesion.Iniciar();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── AbrirParaRegistro: precondición ──────────────────────────────────────

    [Fact]
    public void AbrirParaRegistro_DesdeProgramada_CambiaEstadoAEnPreparacion()
    {
        // Arrange
        var sesion = SesionAggregate.Crear(SesionId.Nuevo(), TipoSesion.BusquedaTesoro);

        // Act
        sesion.AbrirParaRegistro();

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.EnPreparacion);
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
        var sesion = SesionAggregate.Crear(SesionId.Nuevo(), TipoSesion.BusquedaTesoro);

        var propEstado = typeof(SesionAggregate)
            .GetProperty(nameof(SesionAggregate.Estado))!;
        propEstado.GetSetMethod(nonPublic: true)!.Invoke(sesion, [estadoInvalido]);

        // Act
        var act = () => sesion.AbrirParaRegistro();

        // Assert
        act.Should().Throw<DomainException>();
    }
}
