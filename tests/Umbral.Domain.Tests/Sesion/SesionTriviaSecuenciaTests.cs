using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

public sealed class SesionTriviaSecuenciaTests
{
    private const int Duracion = 30;

    [Fact]
    public void LanzarPreguntaTrivia_SesionActivaEtapaTrivia_ActivaTimerYFase()
    {
        var sesion = CrearSesionTriviaActiva(preguntas: 2);
        var ahora = DateTimeOffset.Parse("2026-07-14T18:00:00Z");

        sesion.IniciarSecuenciaTrivia(ahora, Duracion);

        sesion.ContextoMision!.ObtenerFaseTrivia().Should().Be(FaseTrivia.PreguntaActiva);
        sesion.ContextoMision.TimerCerradoEn.Should().Be(ahora.UtcDateTime.AddSeconds(Duracion));
        sesion.ContextoMision.PreguntaTriviaActualIndex.Should().Be(0);
        sesion.HistorialEventos.Should().Contain(e => e.Tipo == "PreguntaTriviaIniciada");
    }

    [Fact]
    public void LanzarPreguntaTrivia_CuandoYaActiva_LanzaDomainException()
    {
        var sesion = CrearSesionTriviaActiva();
        sesion.LanzarPreguntaTrivia(DateTimeOffset.UtcNow, Duracion);

        var act = () => sesion.LanzarPreguntaTrivia(DateTimeOffset.UtcNow, Duracion);

        act.Should().Throw<DomainException>().WithMessage("*activa*");
    }

    [Fact]
    public void LanzarPreguntaTrivia_SesionNoActiva_LanzaDomainException()
    {
        var sesion = CrearSesionTriviaEnPreparacion();

        var act = () => sesion.LanzarPreguntaTrivia(DateTimeOffset.UtcNow, Duracion);

        act.Should().Throw<DomainException>().WithMessage("*activa*");
    }

    [Fact]
    public void CerrarPreguntaTrivia_TrasLanzar_EntraEnTransicion()
    {
        var sesion = CrearSesionTriviaActiva();
        sesion.LanzarPreguntaTrivia(DateTimeOffset.UtcNow, Duracion);

        sesion.CerrarPreguntaTriviaYEntrarTransicion();

        sesion.ContextoMision!.ObtenerFaseTrivia().Should().Be(FaseTrivia.Transicion);
        sesion.ContextoMision.TimerCerradoEn.Should().BeNull();
        sesion.HistorialEventos.Should().Contain(e => e.Tipo == "TriviaEnTransicion");
    }

    [Fact]
    public void CerrarPreguntaTrivia_SinPreguntaActiva_LanzaDomainException()
    {
        var sesion = CrearSesionTriviaActiva();

        var act = () => sesion.CerrarPreguntaTriviaYEntrarTransicion();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void LanzarTrasTransicion_AvanzaASiguientePregunta()
    {
        var sesion = CrearSesionTriviaActiva(preguntas: 2);
        var ahora = DateTimeOffset.UtcNow;
        sesion.LanzarPreguntaTrivia(ahora, Duracion);
        sesion.CerrarPreguntaTriviaYEntrarTransicion();

        sesion.LanzarPreguntaTrivia(ahora.AddMinutes(1), Duracion);

        sesion.ContextoMision!.PreguntaTriviaActualIndex.Should().Be(1);
        sesion.ContextoMision.ObtenerFaseTrivia().Should().Be(FaseTrivia.PreguntaActiva);
    }

    [Fact]
    public void LanzarTrasUltimaPregunta_LanzaDomainException()
    {
        var sesion = CrearSesionTriviaActiva(preguntas: 1);
        sesion.LanzarPreguntaTrivia(DateTimeOffset.UtcNow, Duracion);
        sesion.CerrarPreguntaTriviaYEntrarTransicion();

        var act = () => sesion.LanzarPreguntaTrivia(DateTimeOffset.UtcNow, Duracion);

        act.Should().Throw<DomainException>().WithMessage("*preguntas*");
    }

    [Fact]
    public void ProcesarCiclo_TimerVencido_EntraEnTransicion()
    {
        var sesion = CrearSesionTriviaActiva(preguntas: 2);
        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);

        var resultado = sesion.ProcesarCicloTriviaAutomatico(
            t0.AddSeconds(Duracion + 1),
            Duracion,
            duracionTransicionSegundos: 5);

        resultado.Should().Be(ResultadoCicloTrivia.EntroEnTransicion);
        sesion.ContextoMision!.ObtenerFaseTrivia().Should().Be(FaseTrivia.Transicion);
        sesion.ContextoMision.TransicionHasta.Should().NotBeNull();
    }

    [Fact]
    public void ProcesarCiclo_TrasTransicion_LanzaSiguientePregunta()
    {
        var sesion = CrearSesionTriviaActiva(preguntas: 2);
        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);
        sesion.ProcesarCicloTriviaAutomatico(t0.AddSeconds(Duracion + 1), Duracion, 5);

        var transicionHasta = sesion.ContextoMision!.TransicionHasta;
        var resultado = sesion.ProcesarCicloTriviaAutomatico(
            t0.AddSeconds(Duracion + 1 + 5),
            Duracion,
            5);

        resultado.Should().Be(ResultadoCicloTrivia.SiguientePreguntaLanzada);
        sesion.ContextoMision!.PreguntaTriviaActualIndex.Should().Be(1);
        sesion.ContextoMision.ObtenerFaseTrivia().Should().Be(FaseTrivia.PreguntaActiva);
        sesion.ContextoMision.TimerCerradoEn.Should().Be(transicionHasta!.Value.AddSeconds(Duracion));
    }

    [Fact]
    public void IniciarSecuenciaTrivia_CuandoYaEnCurso_LanzaDomainException()
    {
        var sesion = CrearSesionTriviaActiva();
        sesion.IniciarSecuenciaTrivia(DateTimeOffset.UtcNow, Duracion);

        var act = () => sesion.IniciarSecuenciaTrivia(DateTimeOffset.UtcNow, Duracion);

        act.Should().Throw<DomainException>().WithMessage("*ya está en curso*");
    }

    [Fact]
    public void ProcesarCiclo_TrasUltimaPregunta_ConSiguienteEtapaTrivia_AutoLanza()
    {
        var ids1 = new[] { PreguntaId.Nuevo() };
        var ids2 = new[] { PreguntaId.Nuevo(), PreguntaId.Nuevo() };
        var etapas = new EtapaSnapshotBase[]
        {
            EtapaTriviaSnapshot.Rehydrate(EtapaId.Nuevo(), 1, Array.Empty<CategoriaId>(), ids1, "Cat A"),
            EtapaTriviaSnapshot.Rehydrate(EtapaId.Nuevo(), 2, Array.Empty<CategoriaId>(), ids2, "Cat B"),
        };
        var snapshot = MisionSnapshot.Rehydrate(MisionId.Nuevo(), "Misión 2 etapas", etapas);
        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        sesion.Iniciar();

        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);
        sesion.ProcesarCicloTriviaAutomatico(t0.AddSeconds(Duracion + 1), Duracion, 5);

        var resultado = sesion.ProcesarCicloTriviaAutomatico(
            t0.AddSeconds(Duracion + 1 + 5),
            Duracion,
            5);

        resultado.Should().Be(ResultadoCicloTrivia.SiguientePreguntaLanzada);
        sesion.ContextoMision!.EtapaActualIndex.Should().Be(1);
        sesion.ContextoMision.PreguntaTriviaActualIndex.Should().Be(0);
        sesion.ContextoMision.ObtenerFaseTrivia().Should().Be(FaseTrivia.PreguntaActiva);
    }

    [Fact]
    public void ProcesarCiclo_TrasUltimaPreguntaUltimaEtapa_FinalizaSesion()
    {
        var sesion = CrearSesionTriviaActiva(preguntas: 1);
        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);
        sesion.ProcesarCicloTriviaAutomatico(t0.AddSeconds(Duracion + 1), Duracion, 5);

        var resultado = sesion.ProcesarCicloTriviaAutomatico(
            t0.AddSeconds(Duracion + 1 + 5),
            Duracion,
            5);

        resultado.Should().Be(ResultadoCicloTrivia.SecuenciaCompletada);
        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
        sesion.ContextoMision!.ObtenerFaseTrivia().Should().Be(FaseTrivia.Esperando);
    }

    [Fact]
    public void ProcesarCiclo_TimerVencido_SinRespuestas_IgualEntraEnTransicion()
    {
        var sesion = CrearSesionTriviaActiva(preguntas: 2);
        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);

        var resultado = sesion.ProcesarCicloTriviaAutomatico(
            t0.AddSeconds(Duracion + 1),
            Duracion,
            5);

        resultado.Should().Be(ResultadoCicloTrivia.EntroEnTransicion);
        sesion.Participantes.Sum(p => p.PuntajeTotal.Valor).Should().Be(0);
    }

    private static SesionAR CrearSesionTriviaActiva(int preguntas = 2)
    {
        var sesion = CrearSesionTriviaEnPreparacion(preguntas);
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        sesion.Iniciar();
        return sesion;
    }

    private static SesionAR CrearSesionTriviaEnPreparacion(int preguntas = 2)
    {
        var ids = Enumerable.Range(0, preguntas).Select(_ => PreguntaId.Nuevo()).ToList();
        var snapshot = MisionSnapshot.SoloTrivia(ids, "Categorías test");
        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        return sesion;
    }
}
