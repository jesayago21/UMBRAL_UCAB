using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>HU-10 — Liberación de pistas PorGanador al ganar etapa BT.</summary>
public sealed class SesionLiberarPistasPorGanadorTests
{
    [Fact]
    public void RegistrarEvidencia_GanaEtapa_LiberaPorGanadorDeEtapaSiguienteALosDemas()
    {
        var (sesion, pistaId) = CrearSesionDosEtapasConPistaPorGanadorEnEtapa2();
        var alpha = sesion.Participantes[0];
        var beta = sesion.Participantes[1];

        sesion.RegistrarEvidencia(alpha.ParticipanteId, "QR-E1");

        sesion.ContextoMision!.EtapaActualIndex.Should().Be(1);
        sesion.ContextoMision.PistasEntregadas.Should().ContainSingle();
        var entrega = sesion.ContextoMision.PistasEntregadas[0];
        entrega.PistaId.Should().Be(pistaId);
        entrega.EtapaIndex.Should().Be(1);
        entrega.ParticipanteId.Should().Be(beta.ParticipanteId);

        sesion.DomainEvents.OfType<PistaLiberada>().Should().ContainSingle();
        var evt = sesion.DomainEvents.OfType<PistaLiberada>().Single();
        evt.ParticipanteId.Should().Be(beta.ParticipanteId);
        evt.EtapaIndex.Should().Be(1);
        evt.PistaId.Should().Be(pistaId);
    }

    [Fact]
    public void RegistrarEvidencia_GanaEtapa_NoLiberaPorGanadorAlGanador()
    {
        var (sesion, _) = CrearSesionDosEtapasConPistaPorGanadorEnEtapa2();
        var alpha = sesion.Participantes[0];

        sesion.RegistrarEvidencia(alpha.ParticipanteId, "QR-E1");

        sesion.ContextoMision!.PistasEntregadas
            .Should().NotContain(p => p.ParticipanteId == alpha.ParticipanteId);
    }

    [Fact]
    public void RegistrarEvidencia_CuandoSiguienteEsTrivia_NoLiberaPorGanador()
    {
        var sesion = CrearSesionBtLuegoTriviaConPistaPorGanadorEnTrivia();
        var alpha = sesion.Participantes[0];

        sesion.RegistrarEvidencia(alpha.ParticipanteId, "QR-BT1");

        sesion.ContextoMision!.PistasEntregadas.Should().BeEmpty();
        sesion.DomainEvents.OfType<PistaLiberada>().Should().BeEmpty();
    }

    [Fact]
    public void RegistrarEvidencia_UltimaEtapa_NoLiberaPorGanador()
    {
        var mision = Mision.Crear("Una etapa");
        mision.AgregarEtapaBusquedaTesoro("Única", "QR-U1");
        ((EtapaBusquedaTesoro)mision.Etapas[0])
            .AgregarPista("No debería salir", TipoLiberacion.PorGanador);
        mision.Activar();
        mision.ClearDomainEvents();

        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(mision),
            UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        SesionTestHelpers.UnirParticipante(sesion, "Beta");
        sesion.Iniciar();
        sesion.ClearDomainEvents();

        sesion.RegistrarEvidencia(sesion.Participantes[0].ParticipanteId, "QR-U1");

        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
        sesion.ContextoMision!.PistasEntregadas.Should().BeEmpty();
        sesion.DomainEvents.OfType<PistaLiberada>().Should().BeEmpty();
    }

    [Fact]
    public void LiberarPistasPorGanadorEtapaSiguiente_SegundaInvocacion_NoDuplica()
    {
        var (sesion, _) = CrearSesionDosEtapasConPistaPorGanadorEnEtapa2();
        var alpha = sesion.Participantes[0];
        // Stay on stage 0: liberate manually then again
        var primera = sesion.LiberarPistasPorGanadorEtapaSiguiente(
            alpha.ParticipanteId, DateTimeOffset.UtcNow);
        sesion.ClearDomainEvents();

        var segunda = sesion.LiberarPistasPorGanadorEtapaSiguiente(
            alpha.ParticipanteId, DateTimeOffset.UtcNow);

        primera.Should().Be(1);
        segunda.Should().Be(0);
        sesion.ContextoMision!.PistasEntregadas.Should().HaveCount(1);
    }

    [Fact]
    public void LiberarPistasPorGanadorEtapaSiguiente_CuandoPausada_NoEntrega()
    {
        var (sesion, _) = CrearSesionDosEtapasConPistaPorGanadorEnEtapa2();
        sesion.Pausar();
        sesion.ClearDomainEvents();

        var count = sesion.LiberarPistasPorGanadorEtapaSiguiente(
            sesion.Participantes[0].ParticipanteId, DateTimeOffset.UtcNow);

        count.Should().Be(0);
        sesion.ContextoMision!.PistasEntregadas.Should().BeEmpty();
    }

    private static (SesionAR Sesion, PistaId PistaId) CrearSesionDosEtapasConPistaPorGanadorEnEtapa2()
    {
        var mision = Mision.Crear("PorGanador avance");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-E1");
        mision.AgregarEtapaBusquedaTesoro("Etapa 2", "QR-E2");
        ((EtapaBusquedaTesoro)mision.Etapas[1])
            .AgregarPista("Pista de avance etapa 2", TipoLiberacion.PorGanador);
        mision.Activar();
        mision.ClearDomainEvents();

        var pistaId = ((EtapaBusquedaTesoro)mision.Etapas[1]).Pistas[0].PistaId;
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

    private static SesionAR CrearSesionBtLuegoTriviaConPistaPorGanadorEnTrivia()
    {
        var mision = Mision.Crear("BT luego Trivia");
        mision.AgregarEtapaBusquedaTesoro("BT 1", "QR-BT1");
        mision.AgregarEtapaTrivia([CategoriaId.Nuevo()]);
        mision.Activar();
        mision.ClearDomainEvents();

        var trivia = (EtapaTrivia)mision.Etapas[1];
        var triviaSnap = EtapaTriviaSnapshot.DesdeEtapa(
            trivia,
            [PreguntaId.Nuevo()],
            "Trivia");
        var snapshot = MisionSnapshot.Desde(
            mision,
            new Dictionary<EtapaId, EtapaTriviaSnapshot> { [trivia.EtapaId] = triviaSnap });

        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        SesionTestHelpers.UnirParticipante(sesion, "Beta");
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }
}
