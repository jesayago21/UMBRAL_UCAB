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

        var act = () => ctx.AvanzarEtapa();

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

        var act = () => ctx.AvanzarEtapa();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AvanzarEtapa_ConGanadorBt_IncrementaIndexYReseteaGanador()
    {
        var ctx = ContextoMision.Crear(SesionBuilder.MisionSnapshotFake());
        var ganador = ParticipanteId.Nuevo();
        ctx.RegistrarGanadorEtapa(ganador);

        ctx.AvanzarEtapa();

        ctx.EtapaActualIndex.Should().Be(1);
        ctx.GanadorEtapaActualId.Should().BeNull();
        ctx.YaHayGanadorEnEtapaActual().Should().BeFalse();
        ctx.EsUltimaEtapa().Should().BeTrue();
    }
}
