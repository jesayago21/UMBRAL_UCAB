using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

public sealed class ContextoMisionTests
{
    [Fact]
    public void Crear_ConSnapshotSinEtapas_LanzaDomainException()
    {
        var mision = Mision.Crear("Vacía");
        var snapshot = MisionSnapshot.Rehydrate(
            mision.MisionId,
            mision.Nombre,
            []);

        var act = () => ContextoMision.Crear(snapshot);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ObtenerEtapaActual_IndexFueraDeRango_LanzaDomainException()
    {
        var snapshot = SesionBuilder.MisionSnapshotFake();
        var ctx = ContextoMision.Rehydrate(
            snapshot.MisionId,
            snapshot,
            etapaActualIndex: 99,
            ganadorEtapaActualId: null,
            preguntaTriviaActualIndex: 0);

        var act = () => ctx.ObtenerEtapaActual();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ObtenerEtapaBusquedaTesoroActual_CuandoEtapaTrivia_LanzaDomainException()
    {
        var mision = Mision.Crear("Solo trivia");
        mision.AgregarEtapaTrivia([CategoriaId.Nuevo()]);
        var trivia = (EtapaTrivia)mision.Etapas[0];
        var triviaSnap = EtapaTriviaSnapshot.DesdeEtapa(
            trivia,
            [PreguntaId.Nuevo()],
            "Trivia");
        var snapshot = MisionSnapshot.Desde(
            mision,
            new Dictionary<EtapaId, EtapaTriviaSnapshot> { [trivia.EtapaId] = triviaSnap });
        var ctx = ContextoMision.Crear(snapshot);

        var act = () => ctx.ObtenerEtapaBusquedaTesoroActual();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegistrarGanadorEtapa_SegundoGanador_LanzaDomainException()
    {
        var ctx = ContextoMision.Crear(SesionBuilder.MisionSnapshotFake());
        var ganador = ParticipanteId.Nuevo();
        ctx.RegistrarGanadorEtapa(ganador);

        var act = () => ctx.RegistrarGanadorEtapa(ParticipanteId.Nuevo());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AvanzarEtapa_BtSinGanador_LanzaDomainException()
    {
        var ctx = ContextoMision.Crear(SesionBuilder.MisionSnapshotFake());

        var act = () => ctx.AvanzarEtapa(DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AvanzarEtapa_UltimaEtapa_LanzaDomainException()
    {
        var snapshot = SesionBuilder.MisionSnapshotFake();
        var ctx = ContextoMision.Rehydrate(
            snapshot.MisionId,
            snapshot,
            etapaActualIndex: snapshot.Etapas.Count - 1,
            ganadorEtapaActualId: ParticipanteId.Nuevo(),
            preguntaTriviaActualIndex: 0);

        var act = () => ctx.AvanzarEtapa(DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AvanzarEtapa_ConGanadorBt_IncrementaIndexYReseteaGanador()
    {
        var ctx = ContextoMision.Crear(SesionBuilder.MisionSnapshotFake());
        var ganador = ParticipanteId.Nuevo();
        ctx.RegistrarGanadorEtapa(ganador);

        ctx.AvanzarEtapa(DateTimeOffset.UtcNow);

        ctx.EtapaActualIndex.Should().Be(1);
        ctx.GanadorEtapaActualId.Should().BeNull();
        ctx.YaHayGanadorEnEtapaActual().Should().BeFalse();
        ctx.EsUltimaEtapa().Should().BeTrue();
        ctx.EtapaIniciadaEn.Should().NotBeNull();
    }

    [Fact]
    public void SegundosEfectivosTranscurridos_ConPausaEnCurso_CongelaReloj()
    {
        var snapshot = SesionBuilder.MisionSnapshotFake();
        var t0 = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ctx = ContextoMision.Rehydrate(
            snapshot.MisionId,
            snapshot,
            etapaActualIndex: 0,
            ganadorEtapaActualId: null,
            preguntaTriviaActualIndex: 0,
            etapaIniciadaEn: t0,
            pausadaDesde: t0.AddSeconds(10),
            segundosPausaAcumulados: 0);

        // Wall 50s, en pausa desde el segundo 10 → efectivos = 10.
        ctx.SegundosEfectivosTranscurridos(t0.AddSeconds(50)).Should().Be(10);
        ctx.SegundosEfectivosTranscurridos(t0.AddSeconds(90)).Should().Be(10);
    }

    [Fact]
    public void SegundosEfectivosTranscurridos_ConPausaAcumulada_LaExcluye()
    {
        var snapshot = SesionBuilder.MisionSnapshotFake();
        var t0 = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var ctx = ContextoMision.Rehydrate(
            snapshot.MisionId,
            snapshot,
            etapaActualIndex: 0,
            ganadorEtapaActualId: null,
            preguntaTriviaActualIndex: 0,
            etapaIniciadaEn: t0,
            pausadaDesde: null,
            segundosPausaAcumulados: 40);

        // Wall 60s − 40s pausa = 20s efectivos.
        ctx.SegundosEfectivosTranscurridos(t0.AddSeconds(60)).Should().Be(20);
    }
}
