using FluentAssertions;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision.Events;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.CatalogoBusquedaTesoro;

/// <summary>
/// Tests de Iteracion 1: BC CatalogoBusquedaTesoro — Mision AR y MisionSnapshot.
/// Fuente de verdad: umbral-backend-spec.md §2 + ddd-modeling-skill.md §3.2-3.4
/// </summary>
public sealed class MisionTests
{
    // ── Mision.Crear ──────────────────────────────────────────────────────

    [Fact]
    public void Crear_ConNombreValido_RetornaMisionEnEstadoBorrador()
    {
        // Act
        var mision = Mision.Crear("Gran Cacería");

        // Assert
        mision.Estado.Should().Be(EstadoMision.Borrador);
        mision.Nombre.Should().Be("Gran Cacería");
    }

    [Fact]
    public void Crear_ConNombreValido_MisionIdNoEsDefault()
    {
        // Act
        var mision = Mision.Crear("Test");

        // Assert
        mision.MisionId.Should().NotBeNull();
        mision.MisionId.Valor.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Crear_ConNombreValido_EmiteMisionCreadaEvent()
    {
        // Act
        var mision = Mision.Crear("Test");

        // Assert
        mision.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MisionCreada>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConNombreVacioOEspacios_LanzaDomainException(string nombre)
    {
        // Act
        var act = () => Mision.Crear(nombre);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Crear_NuevaMision_NaceConEtapasVacias()
    {
        // Act
        var mision = Mision.Crear("Test");

        // Assert
        mision.Etapas.Should().BeEmpty();
    }

    // ── Mision.AgregarEtapa ───────────────────────────────────────────────

    [Fact]
    public void AgregarEtapa_ConDatosValidos_IncrementaConteoEtapas()
    {
        // Arrange
        var mision = Mision.Crear("Test");

        // Act
        mision.AgregarEtapa("Busca el árbol rojo", "QR-ARBOL-001");
        mision.AgregarEtapa("Encuentra la fuente", "QR-FUENTE-002");

        // Assert
        mision.Etapas.Should().HaveCount(2);
    }

    [Fact]
    public void AgregarEtapa_AsignaOrdenSecuencialCorrecto()
    {
        // Arrange
        var mision = Mision.Crear("Test");
        mision.AgregarEtapa("Primera", "QR-001");
        mision.AgregarEtapa("Segunda", "QR-002");

        // Assert
        mision.Etapas[0].Orden.Should().Be(1);
        mision.Etapas[1].Orden.Should().Be(2);
    }

    // ── Mision.Activar ────────────────────────────────────────────────────

    [Fact]
    public void Activar_CuandoTieneEtapas_CambiaEstadoAActiva()
    {
        // Arrange
        var mision = Mision.Crear("Test");
        mision.AgregarEtapa("Etapa única", "QR-001");

        // Act
        mision.Activar();

        // Assert
        mision.Estado.Should().Be(EstadoMision.Activa);
    }

    [Fact]
    public void Activar_CuandoTieneEtapas_EmiteMisionActivadaEvent()
    {
        // Arrange
        var mision = Mision.Crear("Test");
        mision.AgregarEtapa("Etapa única", "QR-001");
        mision.ClearDomainEvents();

        // Act
        mision.Activar();

        // Assert
        mision.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MisionActivada>();
    }

    [Fact]
    public void Activar_CuandoNoTieneEtapas_LanzaDomainException()
    {
        // Arrange
        var mision = Mision.Crear("Sin Etapas");

        // Act
        var act = () => mision.Activar();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*al menos una etapa*");
    }

    [Fact]
    public void Activar_CuandoYaEstaActiva_LanzaDomainException()
    {
        // Arrange
        var mision = Mision.Crear("Test");
        mision.AgregarEtapa("Etapa", "QR-001");
        mision.Activar();

        // Act
        var act = () => mision.Activar();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── Mision.PuedeUsarseParaSesion ──────────────────────────────────────

    [Fact]
    public void PuedeUsarseParaSesion_CuandoEstaActiva_RetornaTrue()
    {
        // Arrange
        var mision = Mision.Crear("Test");
        mision.AgregarEtapa("Etapa", "QR-001");
        mision.Activar();

        // Assert
        mision.PuedeUsarseParaSesion().Should().BeTrue();
    }

    [Fact]
    public void PuedeUsarseParaSesion_CuandoEstaBorrador_RetornaFalse()
    {
        // Arrange
        var mision = Mision.Crear("Test");

        // Assert
        mision.PuedeUsarseParaSesion().Should().BeFalse();
    }

    // ── MisionSnapshot.Desde ─────────────────────────────────────────────

    [Fact]
    public void MisionSnapshot_Desde_CopiaIdYNombreDeMision()
    {
        // Arrange
        var mision = Mision.Crear("Jungla Profunda");
        mision.AgregarEtapa("Etapa", "QR-001");
        mision.Activar();

        // Act
        var snapshot = MisionSnapshot.Desde(mision);

        // Assert
        snapshot.MisionId.Should().Be(mision.MisionId);
        snapshot.Nombre.Should().Be("Jungla Profunda");
    }

    [Fact]
    public void MisionSnapshot_Desde_CopiaEtapasEnOrden()
    {
        // Arrange
        var mision = Mision.Crear("Test");
        mision.AgregarEtapa("Segunda en agregar (orden 1 igual)", "QR-001");
        mision.AgregarEtapa("Tercera en agregar (orden 2)", "QR-002");
        mision.Activar();

        // Act
        var snapshot = MisionSnapshot.Desde(mision);

        // Assert
        snapshot.Etapas.Should().HaveCount(2);
        snapshot.Etapas[0].Orden.Should().Be(1);
        snapshot.Etapas[1].Orden.Should().Be(2);
    }

    [Fact]
    public void MisionSnapshot_Desde_CuandoMisionEsNull_LanzaArgumentNullException()
    {
        // Act
        var act = () => MisionSnapshot.Desde(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MisionSnapshot_EsInmutable_ModificacionEnMisionOriginalNoAfectaSnapshot()
    {
        // Arrange
        var mision = Mision.Crear("Test");
        mision.AgregarEtapa("Etapa original", "QR-001");
        mision.Activar();
        var snapshot = MisionSnapshot.Desde(mision);
        var etapasAntes = snapshot.Etapas.Count;

        // Act — modificar mision despues del snapshot
        mision.AgregarEtapa("Etapa nueva post-snapshot", "QR-002");

        // Assert
        snapshot.Etapas.Should().HaveCount(etapasAntes,
            "el snapshot debe ser inmutable al momento de su creacion");
    }
}
