using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Iteración 5 — RegistrarEvidencia (HU-18 + HU-19 parcial).
/// Fuente: umbral-backend-spec.md §6.2, umbral-quality-spec.md, ERS HU-18.
/// </summary>
public sealed class SesionRegistrarEvidenciaTests
{
    private const string QrEtapa1 = "QR-ARBOL-001";
    private const string QrEtapa2 = "QR-FUENTE-002";

    // ── Happy path ─────────────────────────────────────────────────

    [Fact]
    public void RegistrarEvidencia_CuandoQrValido_RegistraEvidenciaConResultadoValida()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConEquipo("Alpha").Build();
        var equipo  = sesion.Equipos.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa1);

        // Assert
        sesion.Evidencias.Should().ContainSingle();
        evidencia.Resultado.Should().Be(ResultadoValidacion.Valida);
        evidencia.EquipoId.Should().Be(equipo.EquipoId);
        evidencia.CodigoQR.Valor.Should().Be(QrEtapa1);
        evidencia.TimestampServidor.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegistrarEvidencia_CuandoQrValido_EmiteEvidenciaRegistradaEvent()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var equipo  = sesion.Equipos.First();

        // Act
        sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa1);

        // Assert
        sesion.DomainEvents.Should().ContainSingle(e => e is EvidenciaRegistrada);
    }

    [Fact]
    public void RegistrarEvidencia_CuandoQrValido_EventoContieneDatosCorrectos()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var equipo  = sesion.Equipos.First();
        var etapa   = sesion.ContextoBT!.ObtenerEtapaActual();

        // Act
        sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa1);

        // Assert
        var evt = sesion.DomainEvents.OfType<EvidenciaRegistrada>().Single();
        evt.SesionId.Should().Be(sesion.SesionId);
        evt.EquipoId.Should().Be(equipo.EquipoId);
        evt.EtapaId.Should().Be(etapa.EtapaId);
        evt.Resultado.Should().Be(ResultadoValidacion.Valida);
        evt.CodigoQR.Should().Be(QrEtapa1);
    }

    [Fact]
    public void RegistrarEvidencia_CuandoQrValido_RegistraEventoEnHistorial()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var equipo  = sesion.Equipos.First();

        // Act
        sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa1);

        // Assert
        sesion.HistorialEventos.Should()
            .Contain(e => e.Tipo == "EvidenciaRegistrada");
    }

    // ── QR inválido (etapa incorrecta) ─────────────────────────────

    [Fact]
    public void RegistrarEvidencia_CuandoQrDeOtraEtapa_RegistraConResultadoInvalida()
    {
        // Arrange — etapa activa es 1, enviamos QR de etapa 2
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var equipo  = sesion.Equipos.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa2);

        // Assert
        evidencia.Resultado.Should().Be(ResultadoValidacion.Invalida);
        sesion.Evidencias.Should().ContainSingle();
    }

    [Fact]
    public void RegistrarEvidencia_CuandoQrInvalido_EmiteEventoConResultadoInvalida()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var equipo  = sesion.Equipos.First();

        // Act
        sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa2);

        // Assert
        sesion.DomainEvents.OfType<EvidenciaRegistrada>().Single()
            .Resultado.Should().Be(ResultadoValidacion.Invalida);
    }

    // ── Sesión no activa (RB-19) ───────────────────────────────────

    [Fact]
    public void RegistrarEvidencia_CuandoSesionPausada_RegistraConResultadoRechazada()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.Pausada).Build();
        var equipo  = sesion.Equipos.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa1);

        // Assert
        evidencia.Resultado.Should().Be(ResultadoValidacion.Rechazada);
        sesion.Evidencias.Should().ContainSingle();
    }

    [Theory]
    [InlineData(EstadoSesion.Finalizada)]
    public void RegistrarEvidencia_CuandoSesionCerrada_RegistraConResultadoRechazada(
        EstadoSesion estado)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().ConEstado(estado).Build();
        var equipo  = sesion.Equipos.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa1);

        // Assert
        evidencia.Resultado.Should().Be(ResultadoValidacion.Rechazada);
    }

    [Fact]
    public void RegistrarEvidencia_CuandoSesionCancelada_RegistraConResultadoRechazada()
    {
        // Arrange — Cancelada no tiene equipos en builder; se construye y cancela explícitamente
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConEquipo("Alpha").Build();
        sesion.Cancelar("Cancelada para test");
        sesion.ClearDomainEvents();
        var equipo = sesion.Equipos.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa1);

        // Assert
        evidencia.Resultado.Should().Be(ResultadoValidacion.Rechazada);
    }

    // ── Guards ─────────────────────────────────────────────────────

    [Fact]
    public void RegistrarEvidencia_CuandoEquipoNoPerteneceSesion_LanzaDomainException()
    {
        // Arrange
        var sesion      = SesionBuilder.BusquedaTesoro().Activa().Build();
        var equipoAjeno = EquipoId.Nuevo();

        // Act
        var act = () => sesion.RegistrarEvidencia(equipoAjeno, QrEtapa1);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RegistrarEvidencia_CuandoQrVacio_LanzaDomainException(string qr)
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var equipo  = sesion.Equipos.First();

        // Act
        var act = () => sesion.RegistrarEvidencia(equipo.EquipoId, qr);

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── HU-19 parcial: duplicado del mismo equipo ──────────────────

    [Fact]
    public void RegistrarEvidencia_CuandoEquipoYaEnvioValidaEnEtapa_RegistraSegundaComoInvalida()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConEquipo("Alpha").Build();
        var equipo  = sesion.Equipos.First();
        sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa1);

        // Act — segundo envío válido del mismo equipo en la misma etapa
        var segunda = sesion.RegistrarEvidencia(equipo.EquipoId, QrEtapa1);

        // Assert
        segunda.Resultado.Should().Be(ResultadoValidacion.Invalida);
        sesion.Evidencias.Should().HaveCount(2);
    }

    [Fact]
    public void RegistrarEvidencia_DosEquiposDistintos_PuedenEnviarQrValido()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConEquipo("Alpha").ConEquipo("Beta").Build();
        var alpha = sesion.Equipos.First(e => e.Nombre.Valor == "Alpha");
        var beta  = sesion.Equipos.First(e => e.Nombre.Valor == "Beta");

        // Act
        var evAlpha = sesion.RegistrarEvidencia(alpha.EquipoId, QrEtapa1);
        var evBeta  = sesion.RegistrarEvidencia(beta.EquipoId, QrEtapa1);

        // Assert — ganador único (RB-04) se implementa en iter-06
        evAlpha.Resultado.Should().Be(ResultadoValidacion.Valida);
        evBeta.Resultado.Should().Be(ResultadoValidacion.Valida);
        sesion.Evidencias.Should().HaveCount(2);
    }
}
