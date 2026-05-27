using FluentAssertions;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using SesionAR = Umbral.Domain.Sesion.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Tests de Iteracion 1: CrearSesionBusquedaTesoro
/// Fuente de verdad: umbral-backend-spec.md §2.4 + umbral-quality-spec.md §4.1
///
/// Reglas cubiertos:
///   R-CB-01: La sesion nace en estado Programada.
///   R-CB-02: TipoSesion == BusquedaTesoro.
///   R-CB-03: Se emite SesionCreada con SesionId, TipoSesion y OperadorId.
///   R-CB-04: ContextoBT != null y contiene el MisionSnapshot.
///   R-CB-05: Dos creaciones generan SesionId distintos (unicidad).
///   R-CB-06: snapshot nulo lanza ArgumentNullException (guard).
///   R-CB-07: operadorId nulo lanza ArgumentNullException (guard).
/// </summary>
public sealed class SesionCrearTests
{
    // ── Happy path ─────────────────────────────────────────────────────────

    [Fact]
    public void CrearBusquedaTesoro_ConDatosValidos_RetornaSesionEnEstadoProgramada()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake();
        var operadorId = UsuarioId.Nuevo();

        // Act
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Programada);
    }

    [Fact]
    public void CrearBusquedaTesoro_ConDatosValidos_TipoSesionEsBusquedaTesoro()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake();
        var operadorId = UsuarioId.Nuevo();

        // Act
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        sesion.TipoSesion.Should().Be(TipoSesion.BusquedaTesoro);
    }

    [Fact]
    public void CrearBusquedaTesoro_ConDatosValidos_OperadorIdAsignado()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake();
        var operadorId = new UsuarioId(Guid.NewGuid());

        // Act
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        sesion.OperadorId.Should().Be(operadorId);
    }

    [Fact]
    public void CrearBusquedaTesoro_ConDatosValidos_SesionIdNoEsDefault()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake();
        var operadorId = UsuarioId.Nuevo();

        // Act
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        sesion.SesionId.Should().NotBeNull();
        sesion.SesionId.Valor.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CrearBusquedaTesoro_DosCalls_GeneranSesionIdsDistintos()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake();
        var operadorId = UsuarioId.Nuevo();

        // Act
        var sesion1 = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);
        var sesion2 = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        sesion1.SesionId.Should().NotBe(sesion2.SesionId);
    }

    [Fact]
    public void CrearBusquedaTesoro_ConDatosValidos_EmiteSesionCreadaEvent()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake();
        var operadorId = UsuarioId.Nuevo();

        // Act
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        sesion.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SesionCreada>();
    }

    [Fact]
    public void CrearBusquedaTesoro_ConDatosValidos_EventoSesionCreadaTienePayloadCorrecto()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake();
        var operadorId = new UsuarioId(Guid.NewGuid());

        // Act
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        var evento = sesion.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SesionCreada>().Subject;

        evento.SesionId.Should().Be(sesion.SesionId);
        evento.TipoSesion.Should().Be(TipoSesion.BusquedaTesoro);
        evento.OperadorId.Should().Be(operadorId);
    }

    [Fact]
    public void CrearBusquedaTesoro_ConDatosValidos_ContextoBTNoEsNull()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake();
        var operadorId = UsuarioId.Nuevo();

        // Act
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        sesion.ContextoBT.Should().NotBeNull();
    }

    [Fact]
    public void CrearBusquedaTesoro_ConDatosValidos_ContextoBTContieneMisionSnapshot()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake("Misión Especial");
        var operadorId = UsuarioId.Nuevo();

        // Act
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        sesion.ContextoBT!.MisionSnapshot.Nombre.Should().Be("Misión Especial");
        sesion.ContextoBT.MisionSnapshot.Etapas.Should().HaveCount(2);
    }

    [Fact]
    public void CrearBusquedaTesoro_ConDatosValidos_SesionNaceConEquiposVacios()
    {
        // Arrange
        var snapshot   = SesionBuilder.MisionSnapshotFake();
        var operadorId = UsuarioId.Nuevo();

        // Act
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);

        // Assert
        sesion.Equipos.Should().BeEmpty();
    }

    // ── Error guards ───────────────────────────────────────────────────────

    [Fact]
    public void CrearBusquedaTesoro_CuandoSnapshotEsNull_LanzaArgumentNullException()
    {
        // Arrange
        var operadorId = UsuarioId.Nuevo();

        // Act
        var act = () => SesionAR.CrearBusquedaTesoro(null!, operadorId);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CrearBusquedaTesoro_CuandoOperadorIdEsNull_LanzaArgumentNullException()
    {
        // Arrange
        var snapshot = SesionBuilder.MisionSnapshotFake();

        // Act
        var act = () => SesionAR.CrearBusquedaTesoro(snapshot, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}

