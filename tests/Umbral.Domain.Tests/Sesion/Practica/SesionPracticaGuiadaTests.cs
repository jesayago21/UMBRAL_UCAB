using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion.Practica;

/// <summary>
/// ARCHIVO DE PRÁCTICA (defensa) — no es requisito de entrega.
///
/// Cómo usarlo:
/// 1. Lee el EJEMPLO RESUELTO.
/// 2. Quita el Skip = "..." de un ejercicio.
/// 3. Completa Arrange / Act / Assert.
/// 4. Corre:
///    dotnet test tests/Umbral.Domain.Tests --filter "FullyQualifiedName~Practica"
///
/// Pistas al final del archivo (no mires hasta intentarlo).
/// </summary>
public sealed class SesionPracticaGuiadaTests
{
    // ═══════════════════════════════════════════════════════════════
    // EJEMPLO RESUELTO — míralo y cópialo mentalmente
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Ejemplo_Pausar_CuandoEstaActiva_CambiaEstadoAPausada()
    {
        // Arrange — preparo una sesión ya Activa
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        // Act — ejecuto el método de dominio
        sesion.Pausar();

        // Assert — verifico el resultado
        sesion.Estado.Should().Be(EstadoSesion.Pausada);
    }

    // ═══════════════════════════════════════════════════════════════
    // EJERCICIO 1 — Happy path (quita el Skip y completa)
    // Meta: reanudar una sesión Pausada → debe quedar Activa
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Practica1_Reanudar_CuandoEstaPausada_CambiaEstadoAActiva()
    {
        // Arrange
        // TODO: crea sesión Pausada con SesionBuilder...ConEstado(EstadoSesion.Pausada).Build()
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(EstadoSesion.Pausada).Build();
        // Act
        // TODO: llama sesion.Reanudar()
        sesion.Reanudar();

        // Assert
        // TODO: sesion.Estado.Should().Be(EstadoSesion.Activa);
        sesion.Estado.Should().Be(EstadoSesion.Activa);
    }

    // ═══════════════════════════════════════════════════════════════
    // EJERCICIO 2 — Caso negativo (excepción)
    // Meta: pausar cuando NO está Activa → DomainException
    // Tip: var act = () => sesion.Pausar();
    //      act.Should().Throw<DomainException>();
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Practica2_Pausar_CuandoEstaEnPreparacion_LanzaDomainException()
    {
        // Arrange
        // TODO: sesión en EnPreparacion
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(EstadoSesion.EnPreparacion).Build();
        // Act + Assert
        var act = () => sesion.Pausar();
        // TODO: Action + Throw<DomainException>
        // Opcional: .WithMessage("*No se puede pausar*")
        act.Should().Throw<DomainException>()
            .WithMessage("*No se puede pausar una sesión en estado*");
    }

    // ═══════════════════════════════════════════════════════════════
    // EJERCICIO 3 — Evento de dominio
    // Meta: al Pausar, debe existir un DomainEvent SesionPausada
    // Tip: sesion.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SesionPausada>();
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Practica3_Pausar_CuandoEstaActiva_EmiteSesionPausada()
    {
        // Arrange / Act / Assert
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        sesion.Pausar();
        sesion.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<SesionPausada>();
        // TODO
    }

    // ═══════════════════════════════════════════════════════════════
    // EJERCICIO 4 — Flujo corto (sin mirar SesionCicloVidaTests)
    // Meta: Activa → Pausar → Reanudar → estado final Activa
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Practica4_PausarYReanudar_TerminaActiva()
    {
        // TODO
       var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
       sesion.Pausar();
       sesion.Reanudar();
       sesion.Estado.Should().Be(EstadoSesion.Activa);
    }
}

/*
═══════════════════════════════════════════════════════════════════
SOLUCIONES (mira solo si te trabas)
═══════════════════════════════════════════════════════════════════

--- Practica1 ---
var sesion = SesionBuilder.BusquedaTesoro().ConEstado(EstadoSesion.Pausada).Build();
sesion.Reanudar();
sesion.Estado.Should().Be(EstadoSesion.Activa);

--- Practica2 ---
var sesion = SesionBuilder.BusquedaTesoro().ConEstado(EstadoSesion.EnPreparacion).Build();
var act = () => sesion.Pausar();
act.Should().Throw<DomainException>();

--- Practica3 ---
var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
sesion.Pausar();
sesion.DomainEvents.Should().ContainSingle()
    .Which.Should().BeOfType<SesionPausada>();

--- Practica4 ---
var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
sesion.Pausar();
sesion.Reanudar();
sesion.Estado.Should().Be(EstadoSesion.Activa);

═══════════════════════════════════════════════════════════════════
*/
