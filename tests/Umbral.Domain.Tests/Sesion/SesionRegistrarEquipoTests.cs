using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using SesionAR = Umbral.Domain.Sesion.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Iteración 2 — RegistrarEquipo (HU-13).
/// Fuente: umbral-backend-spec.md §2.4, umbral-quality-spec.md §4.1 (Equipos).
/// </summary>
public sealed class SesionRegistrarEquipoTests
{
    // ── Happy path ─────────────────────────────────────────────────────────

    [Fact]
    public void RegistrarEquipo_CuandoNombreUnico_AgregaEquipo()
    {
        // Arrange
        var sesion = SesionEnPreparacion();

        // Act
        var equipo = sesion.RegistrarEquipo("Los Sabuesos");

        // Assert
        sesion.Equipos.Should().ContainSingle();
        equipo.Nombre.Valor.Should().Be("Los Sabuesos");
        equipo.CodigoAcceso.Should().NotBeNull();
        equipo.CodigoAcceso.Valor.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void RegistrarEquipo_CuandoNombreUnico_AsociaSesionIdDelAgregado()
    {
        // Arrange
        var sesion = SesionEnPreparacion();

        // Act
        var equipo = sesion.RegistrarEquipo("Alpha");

        // Assert
        equipo.SesionId.Should().Be(sesion.SesionId);
        equipo.EquipoId.Valor.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void RegistrarEquipo_CuandoNombreUnico_PuntajeInicialEsCero()
    {
        // Arrange
        var sesion = SesionEnPreparacion();

        // Act
        var equipo = sesion.RegistrarEquipo("Beta");

        // Assert
        equipo.PuntajeTotal.Valor.Should().Be(0);
    }

    [Fact]
    public void RegistrarEquipo_CuandoDosNombresDistintos_AgregaDosEquipos()
    {
        // Arrange
        var sesion = SesionEnPreparacion();

        // Act
        sesion.RegistrarEquipo("Alpha");
        sesion.RegistrarEquipo("Beta");

        // Assert
        sesion.Equipos.Should().HaveCount(2);
    }

    [Fact]
    public void RegistrarEquipo_CuandoDosEquipos_CodigosAccesoSonDistintos()
    {
        // Arrange
        var sesion = SesionEnPreparacion();

        // Act
        var a = sesion.RegistrarEquipo("Alpha");
        var b = sesion.RegistrarEquipo("Beta");

        // Assert
        a.CodigoAcceso.Valor.Should().NotBe(b.CodigoAcceso.Valor);
    }

    [Theory]
    [InlineData(EstadoSesion.Programada)]
    [InlineData(EstadoSesion.EnPreparacion)]
    public void RegistrarEquipo_CuandoSesionNoEstaCerrada_PermiteRegistro(
        EstadoSesion estado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(estado).Build();

        // Act
        var act = () => sesion.RegistrarEquipo("Nuevo Equipo");

        // Assert
        act.Should().NotThrow();
        sesion.Equipos.Should().ContainSingle();
    }

    [Fact]
    public void RegistrarEquipo_CuandoNombreConEspacios_AplicaTrim()
    {
        // Arrange
        var sesion = SesionEnPreparacion();

        // Act
        var equipo = sesion.RegistrarEquipo("  Los Exploradores  ");

        // Assert
        equipo.Nombre.Valor.Should().Be("Los Exploradores");
    }

    [Fact]
    public void RegistrarEquipo_CuandoNombreUnico_RegistraEventoEnHistorial()
    {
        // Arrange
        var sesion = SesionEnPreparacion();

        // Act
        sesion.RegistrarEquipo("Gamma");

        // Assert
        sesion.HistorialEventos.Should().ContainSingle(e =>
            e.Tipo == "EquipoRegistrado" && e.Payload == "Gamma");
    }

    // ── Errores ────────────────────────────────────────────────────────────

    [Fact]
    public void RegistrarEquipo_CuandoNombreDuplicado_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .ConEquipo("Los Sabuesos")
            .Build();

        // Act
        var act = () => sesion.RegistrarEquipo("Los Sabuesos");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Ya existe un equipo*");
    }

    [Fact]
    public void RegistrarEquipo_CuandoDuplicadoIgnoraMayusculas_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionEnPreparacion();
        sesion.RegistrarEquipo("Los Sabuesos");

        // Act
        var act = () => sesion.RegistrarEquipo("los sabuesos");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Ya existe un equipo*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RegistrarEquipo_CuandoNombreVacio_LanzaDomainException(string nombre)
    {
        // Arrange
        var sesion = SesionEnPreparacion();

        // Act
        var act = () => sesion.RegistrarEquipo(nombre);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*no puede estar vacío*");
    }

    [Theory]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void RegistrarEquipo_CuandoSesionCerrada_LanzaDomainException(
        EstadoSesion estadoCerrado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(estadoCerrado)
            .Build();

        // Act
        var act = () => sesion.RegistrarEquipo("Nuevo Equipo");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*sesión cerrada*");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static SesionAR SesionEnPreparacion() =>
        SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .SinEquipos()
            .Build();
}
