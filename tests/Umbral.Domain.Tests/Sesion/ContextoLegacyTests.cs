using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

public sealed class ContextoLegacyTests
{
    [Fact]
    public void ContextoBusquedaTesoro_CrearYAvanzar_ConGanador()
    {
        var snapshot = SesionBuilder.MisionSnapshotFake();
        var ctx = ContextoBusquedaTesoro.Crear(snapshot);
        var ganador = ParticipanteId.Nuevo();

        ctx.ObtenerEtapaActual().CodigoQRSolucion.Should().Be("QR-ARBOL-001");
        ctx.EsUltimaEtapa().Should().BeFalse();
        ctx.RegistrarGanadorEtapa(ganador);
        ctx.YaHayGanadorEnEtapaActual().Should().BeTrue();

        ctx.AvanzarEtapa();

        ctx.EtapaActualIndex.Should().Be(1);
        ctx.GanadorEtapaActualId.Should().BeNull();
    }

    [Fact]
    public void ContextoBusquedaTesoro_AvanzarSinGanador_LanzaDomainException()
    {
        var ctx = ContextoBusquedaTesoro.Crear(SesionBuilder.MisionSnapshotFake());

        var act = () => ctx.AvanzarEtapa();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ContextoTrivia_Crear_ConDatosValidos()
    {
        var ctx = ContextoTrivia.Crear(
            [PreguntaId.Nuevo(), PreguntaId.Nuevo()],
            "Historia, Ciencia");

        ctx.TotalPreguntas.Should().Be(2);
        ctx.CategoriasTitulo.Should().Be("Historia, Ciencia");
        ctx.PreguntaActualIndex.Should().Be(0);
    }

    [Fact]
    public void ContextoTrivia_Crear_SinPreguntas_LanzaDomainException()
    {
        var act = () => ContextoTrivia.Crear([], "Vacía");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ContextoTrivia_Rehydrate_ConTituloVacio_UsaDefault()
    {
        var ctx = ContextoTrivia.Rehydrate(
            [PreguntaId.Nuevo()],
            preguntaActualIndex: 0,
            timerCerradoEn: null,
            categoriasTitulo: "   ");

        ctx.CategoriasTitulo.Should().Be("Trivia");
    }
}
