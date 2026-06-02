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
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConParticipante("Alpha").Build();
        var participante  = sesion.Participantes.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        sesion.Evidencias.Should().ContainSingle();
        evidencia.Resultado.Should().Be(ResultadoValidacion.Valida);
        evidencia.ParticipanteId.Should().Be(participante.ParticipanteId);
        evidencia.CodigoQR.Valor.Should().Be(QrEtapa1);
        evidencia.TimestampServidor.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegistrarEvidencia_CuandoQrValido_EmiteEvidenciaRegistradaEvent()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        sesion.DomainEvents.Should().ContainSingle(e => e is EvidenciaRegistrada);
    }

    [Fact]
    public void RegistrarEvidencia_CuandoQrValido_EventoContieneDatosCorrectos()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();
        var etapa   = sesion.ContextoMision!.ObtenerEtapaBusquedaTesoroActual();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        var evt = sesion.DomainEvents.OfType<EvidenciaRegistrada>().Single();
        evt.SesionId.Should().Be(sesion.SesionId);
        evt.ParticipanteId.Should().Be(participante.ParticipanteId);
        evt.EtapaId.Should().Be(etapa.EtapaId);
        evt.Resultado.Should().Be(ResultadoValidacion.Valida);
        evt.CodigoQR.Should().Be(QrEtapa1);
    }

    [Fact]
    public void RegistrarEvidencia_CuandoQrValido_RegistraEventoEnHistorial()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

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
        var participante  = sesion.Participantes.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa2);

        // Assert
        evidencia.Resultado.Should().Be(ResultadoValidacion.Invalida);
        sesion.Evidencias.Should().ContainSingle();
    }

    [Fact]
    public void RegistrarEvidencia_CuandoQrInvalido_EmiteEventoConResultadoInvalida()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa2);

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
        var participante  = sesion.Participantes.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

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
        var participante  = sesion.Participantes.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        evidencia.Resultado.Should().Be(ResultadoValidacion.Rechazada);
    }

    [Fact]
    public void RegistrarEvidencia_CuandoSesionCancelada_RegistraConResultadoRechazada()
    {
        // Arrange — Cancelada no tiene participantes en builder; se construye y cancela explícitamente
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConParticipante("Alpha").Build();
        sesion.Cancelar("Cancelada para test");
        sesion.ClearDomainEvents();
        var participante = sesion.Participantes.First();

        // Act
        var evidencia = sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        evidencia.Resultado.Should().Be(ResultadoValidacion.Rechazada);
    }

    // ── Guards ─────────────────────────────────────────────────────

    [Fact]
    public void RegistrarEvidencia_CuandoParticipanteNoPerteneceSesion_LanzaDomainException()
    {
        // Arrange
        var sesion      = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participanteAjeno = ParticipanteId.Nuevo();

        // Act
        var act = () => sesion.RegistrarEvidencia(participanteAjeno, QrEtapa1);

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
        var participante  = sesion.Participantes.First();

        // Act
        var act = () => sesion.RegistrarEvidencia(participante.ParticipanteId, qr);

        // Assert
        act.Should().Throw<DomainException>();
    }

    // ── HU-19 parcial: duplicado del mismo participante ──────────────────

    [Fact]
    public void RegistrarEvidencia_CuandoParticipanteYaEnvioValidaEnEtapa_RegistraSegundaComoInvalida()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConParticipante("Alpha").Build();
        var participante  = sesion.Participantes.First();
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Act — segundo envío válido del mismo participante en la misma etapa
        var segunda = sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        segunda.Resultado.Should().Be(ResultadoValidacion.Invalida);
        sesion.Evidencias.Should().HaveCount(2);
    }

    [Fact]
    public void RegistrarEvidencia_DosParticipantesDistintos_SoloPrimeroEnEtapaGanaPuntaje()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Alpha").ConParticipante("Beta").Build();
        var alpha = sesion.Participantes.First(e => e.Nombre.Valor == "Alpha");
        var beta  = sesion.Participantes.First(e => e.Nombre.Valor == "Beta");

        // Act
        var evAlpha = sesion.RegistrarEvidencia(alpha.ParticipanteId, QrEtapa1);
        var evBeta  = sesion.RegistrarEvidencia(beta.ParticipanteId, QrEtapa1);

        // Assert — Alpha gana etapa 1; Beta envía QR de etapa ya superada (RB-04/RB-05)
        evAlpha.Resultado.Should().Be(ResultadoValidacion.Valida);
        evBeta.Resultado.Should().Be(ResultadoValidacion.Invalida);
        alpha.PuntajeTotal.Valor.Should().Be(100);
        beta.PuntajeTotal.Valor.Should().Be(0);
    }
}
