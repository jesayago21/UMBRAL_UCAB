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

/// <summary>RF-15 — Pista ad-hoc escrita por el operador.</summary>
public sealed class SesionLiberarPistaManualTests
{
    [Fact]
    public void LiberarPistaManual_ATodos_EntregaACadaParticipante()
    {
        var sesion = CrearSesionActivaDosParticipantes();

        var count = sesion.LiberarPistaManual(
            "Miren el patio",
            participanteId: null,
            DateTimeOffset.UtcNow);

        count.Should().Be(2);
        sesion.ContextoMision!.PistasEntregadas.Should().HaveCount(2);
        sesion.ContextoMision.PistasEntregadas
            .Should().OnlyContain(p => p.Contenido == "Miren el patio");
        sesion.DomainEvents.OfType<PistaLiberada>().Should().HaveCount(2);
        sesion.DomainEvents.OfType<PistaLiberada>()
            .Should().OnlyContain(e => e.Contenido == "Miren el patio");
    }

    [Fact]
    public void LiberarPistaManual_AUnParticipante_SoloEntregaAEse()
    {
        var sesion = CrearSesionActivaDosParticipantes();
        var alpha = sesion.Participantes[0];
        var beta = sesion.Participantes[1];

        var count = sesion.LiberarPistaManual(
            "Solo para Alpha",
            alpha.ParticipanteId,
            DateTimeOffset.UtcNow);

        count.Should().Be(1);
        sesion.ContextoMision!.PistasEntregadas.Should().ContainSingle();
        sesion.ContextoMision.PistasEntregadas[0].ParticipanteId
            .Should().Be(alpha.ParticipanteId);
        sesion.ContextoMision.PistasEntregadas
            .Should().NotContain(p => p.ParticipanteId == beta.ParticipanteId);
    }

    [Fact]
    public void LiberarPistaManual_CuandoPausada_LanzaDomainException()
    {
        var sesion = CrearSesionActivaDosParticipantes();
        sesion.Pausar();
        sesion.ClearDomainEvents();

        var act = () => sesion.LiberarPistaManual(
            "No debería",
            null,
            DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>()
            .WithMessage("*activa*");
        sesion.ContextoMision!.PistasEntregadas.Should().BeEmpty();
    }

    [Fact]
    public void LiberarPistaManual_ContenidoVacio_LanzaDomainException()
    {
        var sesion = CrearSesionActivaDosParticipantes();

        var act = () => sesion.LiberarPistaManual("   ", null, DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>()
            .WithMessage("*contenido*");
    }

    [Fact]
    public void LiberarPistaManual_CuandoEtapaTrivia_LanzaDomainException()
    {
        var sesion = CrearSesionActivaSoloTrivia();

        var act = () => sesion.LiberarPistaManual(
            "En trivia no",
            null,
            DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>()
            .WithMessage("*búsqueda*");
    }

    [Fact]
    public void LiberarPistaManual_DosEnvios_GeneranIdsDistintos()
    {
        var sesion = CrearSesionActivaDosParticipantes();
        var alpha = sesion.Participantes[0];

        sesion.LiberarPistaManual("Primera", alpha.ParticipanteId, DateTimeOffset.UtcNow);
        sesion.LiberarPistaManual("Segunda", alpha.ParticipanteId, DateTimeOffset.UtcNow);

        sesion.ContextoMision!.PistasEntregadas.Should().HaveCount(2);
        sesion.ContextoMision.PistasEntregadas[0].PistaId
            .Should().NotBe(sesion.ContextoMision.PistasEntregadas[1].PistaId);
    }

    private static SesionAR CrearSesionActivaDosParticipantes()
    {
        var mision = Mision.Crear("RF-15 BT");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-RF15");
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
        return sesion;
    }

    private static SesionAR CrearSesionActivaSoloTrivia()
    {
        var mision = Mision.Crear("Solo trivia RF-15");
        mision.AgregarEtapaTrivia([CategoriaId.Nuevo()]);
        var trivia = (EtapaTrivia)mision.Etapas[0];
        var snap = EtapaTriviaSnapshot.DesdeEtapa(
            trivia,
            [PreguntaId.Nuevo()],
            "Trivia");
        var snapshot = MisionSnapshot.Desde(
            mision,
            new Dictionary<EtapaId, EtapaTriviaSnapshot> { [trivia.EtapaId] = snap });

        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }
}
