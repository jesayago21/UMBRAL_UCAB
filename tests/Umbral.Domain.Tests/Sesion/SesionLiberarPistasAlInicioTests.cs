using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// Pistas PorGanador (= al inicio de etapa): se liberan al iniciar / avanzar etapa,
/// junto con el mapa, a todos los participantes.
/// </summary>
public sealed class SesionLiberarPistasAlInicioTests
{
    [Fact]
    public void Iniciar_LiberaPistasAlInicioDeEtapaActualATodos()
    {
        var (sesion, pistaId) = CrearSesionEnPreparacionConPistaAlInicio();
        var alpha = sesion.Participantes[0];
        var beta = sesion.Participantes[1];

        sesion.Iniciar();

        sesion.ContextoMision!.PistasEntregadas.Should().HaveCount(2);
        sesion.ContextoMision.PistasEntregadas.Should().OnlyContain(e =>
            e.PistaId == pistaId && e.EtapaIndex == 0);
        sesion.ContextoMision.PistasEntregadas.Select(e => e.ParticipanteId)
            .Should().BeEquivalentTo([alpha.ParticipanteId, beta.ParticipanteId]);
        sesion.DomainEvents.OfType<PistaLiberada>().Should().HaveCount(2);
    }

    [Fact]
    public void RegistrarEvidencia_AlAvanzarEtapa_LiberaPistaAlInicioDeLaNueva()
    {
        var (sesion, pistaEtapa2Id) = CrearSesionActivaDosEtapasConPistaAlInicioEnEtapa2();
        var alpha = sesion.Participantes[0];
        var beta = sesion.Participantes[1];

        sesion.RegistrarEvidencia(alpha.ParticipanteId, "QR-E1");

        sesion.ContextoMision!.EtapaActualIndex.Should().Be(1);
        var entregasEtapa2 = sesion.ContextoMision.PistasEntregadas
            .Where(e => e.EtapaIndex == 1)
            .ToList();
        entregasEtapa2.Should().HaveCount(2);
        entregasEtapa2.Should().OnlyContain(e => e.PistaId == pistaEtapa2Id);
        entregasEtapa2.Select(e => e.ParticipanteId)
            .Should().BeEquivalentTo([alpha.ParticipanteId, beta.ParticipanteId]);
    }

    [Fact]
    public void LiberarPistasAlInicioEtapaActual_SegundaInvocacion_NoDuplica()
    {
        var (sesion, _) = CrearSesionEnPreparacionConPistaAlInicio();
        sesion.Iniciar();
        sesion.ClearDomainEvents();

        var segunda = sesion.LiberarPistasAlInicioEtapaActual(DateTimeOffset.UtcNow);

        segunda.Should().Be(0);
        sesion.ContextoMision!.PistasEntregadas.Should().HaveCount(2);
    }

    [Fact]
    public void LiberarPistasAlInicioEtapaActual_CuandoPausada_NoEntrega()
    {
        var (sesion, _) = CrearSesionEnPreparacionConPistaAlInicio();
        sesion.Iniciar();
        // Limpiar entregas simulando estado sin liberación (no aplicable); pausar y forzar.
        sesion.Pausar();

        var count = sesion.LiberarPistasAlInicioEtapaActual(DateTimeOffset.UtcNow);

        count.Should().Be(0);
    }

    private static (SesionAR Sesion, PistaId PistaId) CrearSesionEnPreparacionConPistaAlInicio()
    {
        var mision = Mision.Crear("Al inicio etapa 1");
        mision.AgregarEtapaBusquedaTesoro("E1", "QR-E1");
        var etapa = mision.Etapas.OfType<EtapaBusquedaTesoro>().Single();
        etapa.AgregarPista("Pista inicial con el mapa", TipoLiberacion.PorGanador);
        var pistaId = etapa.Pistas.Single().PistaId;
        mision.Activar();

        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(mision),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        SesionTestHelpers.UnirParticipante(sesion, "Beta");
        return (sesion, pistaId);
    }

    private static (SesionAR Sesion, PistaId PistaId) CrearSesionActivaDosEtapasConPistaAlInicioEnEtapa2()
    {
        var mision = Mision.Crear("Al inicio avance");
        mision.AgregarEtapaBusquedaTesoro("E1", "QR-E1");
        mision.AgregarEtapaBusquedaTesoro("E2", "QR-E2");
        var etapa2 = mision.Etapas.OfType<EtapaBusquedaTesoro>().ElementAt(1);
        etapa2.AgregarPista("Pista inicial etapa 2", TipoLiberacion.PorGanador);
        var pistaId = etapa2.Pistas.Single().PistaId;
        mision.Activar();

        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(mision),
            UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        SesionTestHelpers.UnirParticipante(sesion, "Beta");
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return (sesion, pistaId);
    }
}
