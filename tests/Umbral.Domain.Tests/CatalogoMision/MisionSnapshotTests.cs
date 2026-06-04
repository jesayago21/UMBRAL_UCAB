using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.CatalogoMision;

public sealed class MisionSnapshotTests
{
    [Fact]
    public void DesdeSoloBusquedaTesoro_CopiaEtapasBt()
    {
        var mision = Mision.Crear("Snapshot BT");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-001");
        mision.Activar();

        var snapshot = MisionSnapshot.DesdeSoloBusquedaTesoro(mision);

        snapshot.MisionId.Should().Be(mision.MisionId);
        snapshot.Nombre.Should().Be("Snapshot BT");
        snapshot.Etapas.Should().ContainSingle().Which.Should().BeOfType<EtapaBusquedaTesoroSnapshot>();
    }

    [Fact]
    public void Desde_ConEtapaTriviaResuelta_IncluyeSnapshotTrivia()
    {
        var mision = Mision.Crear("Snapshot trivia");
        mision.AgregarEtapaTrivia([CategoriaId.Nuevo()]);
        var trivia = (EtapaTrivia)mision.Etapas[0];
        var preguntaId = PreguntaId.Nuevo();
        var triviaSnap = EtapaTriviaSnapshot.DesdeEtapa(
            trivia,
            [preguntaId],
            "Historia");

        var snapshot = MisionSnapshot.Desde(
            mision,
            new Dictionary<EtapaId, EtapaTriviaSnapshot> { [trivia.EtapaId] = triviaSnap });

        snapshot.Etapas.Should().ContainSingle().Which.Should().BeOfType<EtapaTriviaSnapshot>();
    }

    [Fact]
    public void Desde_SinResolverTrivia_LanzaDomainException()
    {
        var mision = Mision.Crear("Sin resolver");
        mision.AgregarEtapaTrivia([CategoriaId.Nuevo()]);

        var act = () => MisionSnapshot.Desde(mision, new Dictionary<EtapaId, EtapaTriviaSnapshot>());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void EtapaTriviaSnapshot_SinPreguntas_LanzaDomainException()
    {
        var mision = Mision.Crear("Trivia vacía");
        mision.AgregarEtapaTrivia([CategoriaId.Nuevo()]);
        var trivia = (EtapaTrivia)mision.Etapas[0];

        var act = () => EtapaTriviaSnapshot.DesdeEtapa(trivia, [], "Vacía");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Pista_CrearPorTiempoSinSegundos_LanzaDomainException()
    {
        var act = () => Pista.Crear(EtapaId.Nuevo(), "Contenido", TipoLiberacion.PorTiempo, null);

        act.Should().Throw<DomainException>();
    }
}
