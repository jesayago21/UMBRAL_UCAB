using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Tests.Sesion.Builders;
using SesionAR = Umbral.Domain.Sesion.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Iteración 6 — ganador único (HU-19, RB-04) + transición de etapa (HU-20, RB-05).
/// </summary>
public sealed class SesionGanadorTransicionTests
{
    private const string QrEtapa1 = "QR-ARBOL-001";
    private const string QrEtapa2 = "QR-FUENTE-002";

    [Fact]
    public void RegistrarEvidencia_PrimerParticipanteValido_Asigna100Puntos()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConParticipante("Alpha").Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        participante.PuntajeTotal.Valor.Should().Be(100);
    }

    [Fact]
    public void RegistrarEvidencia_PrimerParticipanteValido_EmiteEvidenciaValidada()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        sesion.DomainEvents.Should().Contain(e => e is EvidenciaValidada);
    }

    [Fact]
    public void RegistrarEvidencia_PrimerParticipanteValido_EmiteEtapaCompletada()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        sesion.DomainEvents.Should().Contain(e => e is EtapaCompletada);
        sesion.HistorialEventos.Should().Contain(e => e.Tipo == "EtapaCompletada");
    }

    [Fact]
    public void RegistrarEvidencia_PrimerParticipanteValido_AvancesEtapaActiva()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        sesion.ContextoMision!.EtapaActualIndex.Should().Be(1);
        sesion.ContextoMision.ObtenerEtapaBusquedaTesoroActual().CodigoQRSolucion.Should().Be(QrEtapa2);
    }

    [Fact]
    public void RegistrarEvidencia_SegundoEquipoTrasGanadorEnEtapaIntermedia_NoSumaPuntaje()
    {
        // Arrange — misión de 3 etapas para probar etapa intermedia (no final)
        var sesion = CrearSesionTresEtapasActiva("Alpha", "Beta");
        var alpha = sesion.Participantes.First(e => e.Nombre.Valor == "Alpha");
        var beta  = sesion.Participantes.First(e => e.Nombre.Valor == "Beta");

        sesion.RegistrarEvidencia(alpha.ParticipanteId, "QR-E1"); // avanza a etapa 2
        sesion.RegistrarEvidencia(alpha.ParticipanteId, "QR-E2"); // avanza a etapa 3, alpha 200 pts

        // Act — Beta intenta QR de etapa 2 ya superada (sesión en etapa 3)
        var evBeta = sesion.RegistrarEvidencia(beta.ParticipanteId, "QR-E2");

        // Assert
        evBeta.Resultado.Should().Be(ResultadoValidacion.Invalida);
        beta.PuntajeTotal.Valor.Should().Be(0);
        alpha.PuntajeTotal.Valor.Should().Be(200);
    }

    [Fact]
    public void RegistrarEvidencia_SegundoEquipoTrasGanadorUltimaEtapa_RecibeRechazada()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Alpha").ConParticipante("Beta").Build();
        var alpha = sesion.Participantes.First(e => e.Nombre.Valor == "Alpha");
        var beta  = sesion.Participantes.First(e => e.Nombre.Valor == "Beta");

        sesion.RegistrarEvidencia(alpha.ParticipanteId, QrEtapa1);
        sesion.RegistrarEvidencia(alpha.ParticipanteId, QrEtapa2); // finaliza sesión

        // Act
        var evBeta = sesion.RegistrarEvidencia(beta.ParticipanteId, QrEtapa2);

        // Assert
        evBeta.Resultado.Should().Be(ResultadoValidacion.Rechazada);
        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
    }

    [Fact]
    public void RegistrarEvidencia_CompletarUltimaEtapa_FinalizaSesionAutomaticamente()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConParticipante("Alpha").Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa2);

        // Assert
        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
        sesion.FinalizadaEn.Should().NotBeNull();
        sesion.DomainEvents.Should().Contain(e => e is SesionFinalizada);
    }

    [Fact]
    public void RegistrarEvidencia_GanadorEtapa2_EventoContieneIndexCorrecto()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);
        sesion.ClearDomainEvents();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa2);

        // Assert
        var evt = sesion.DomainEvents.OfType<EtapaCompletada>().Single();
        evt.EtapaCompletadaIndex.Should().Be(1);
    }

    [Fact]
    public void RegistrarEvidencia_EvidenciaValidada_ContienePuntaje100()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);

        // Assert
        var evt = sesion.DomainEvents.OfType<EvidenciaValidada>().Single();
        evt.PuntosOtorgados.Valor.Should().Be(100);
        evt.ParticipanteGanadorId.Should().Be(participante.ParticipanteId);
    }

    [Fact]
    public void RegistrarEvidencia_CompletarDosEtapas_Acumula200Puntos()
    {
        // Arrange
        var sesion = SesionBuilder.BusquedaTesoro().Activa().ConParticipante("Alpha").Build();
        var participante  = sesion.Participantes.First();

        // Act
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa1);
        sesion.RegistrarEvidencia(participante.ParticipanteId, QrEtapa2);

        // Assert
        participante.PuntajeTotal.Valor.Should().Be(200);
    }

    private static SesionAR CrearSesionTresEtapasActiva(params string[] participantes)
    {
        var mision = Mision.Crear("Misión 3 etapas");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-E1");
        mision.AgregarEtapaBusquedaTesoro("Etapa 2", "QR-E2");
        mision.AgregarEtapaBusquedaTesoro("Etapa 3", "QR-E3");
        mision.Activar();
        mision.ClearDomainEvents();

        var snapshot = MisionSnapshot.DesdeSoloBusquedaTesoro(mision);
        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        sesion.AbrirParaRegistro();
        foreach (var nombre in participantes)
            SesionTestHelpers.UnirParticipante(sesion, nombre);
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }
}
