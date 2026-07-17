using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

public sealed class RankingServiceTests
{
    private const int Duracion = 30;

    [Fact]
    public void Calcular_CuandoParticipantesConDistintoPuntaje_OrdenaDescendente()
    {
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Alpha").ConParticipante("Beta").Build();
        var alpha = sesion.Participantes.First(e => e.Nombre.Valor == "Alpha");
        var beta  = sesion.Participantes.First(e => e.Nombre.Valor == "Beta");
        alpha.SumarPuntaje(50);
        beta.SumarPuntaje(100);

        var ranking = RankingService.Calcular(sesion.Participantes);

        ranking.Should().HaveCount(2);
        ranking[0].NombreParticipante.Should().Be("Beta");
        ranking[0].PuntajeTotal.Should().Be(100);
        ranking[0].Posicion.Should().Be(1);
        ranking[1].NombreParticipante.Should().Be("Alpha");
        ranking[1].PuntajeTotal.Should().Be(50);
        ranking[1].Posicion.Should().Be(2);
    }

    [Fact]
    public void Calcular_CuandoEmpateSinTiemposTrivia_OrdenaPorNombre()
    {
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Gamma").ConParticipante("Alpha").Build();
        sesion.Participantes.First(e => e.Nombre.Valor == "Gamma").SumarPuntaje(50);
        sesion.Participantes.First(e => e.Nombre.Valor == "Alpha").SumarPuntaje(50);

        var ranking = RankingService.Calcular(sesion.Participantes);

        ranking[0].NombreParticipante.Should().Be("Alpha");
        ranking[0].Posicion.Should().Be(1);
        ranking[0].TiempoAcumuladoMs.Should().Be(0);
        ranking[1].NombreParticipante.Should().Be("Gamma");
        ranking[1].Posicion.Should().Be(2);
    }

    [Fact]
    public void Calcular_CuandoEmpateEnPuntaje_MenorTiempoAcumuladoGana()
    {
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Alpha").ConParticipante("Beta").Build();
        var alpha = sesion.Participantes.First(e => e.Nombre.Valor == "Alpha");
        var beta  = sesion.Participantes.First(e => e.Nombre.Valor == "Beta");
        alpha.SumarPuntaje(100);
        beta.SumarPuntaje(100);

        var sesionId = sesion.SesionId;
        var preguntaId = PreguntaId.Nuevo();
        var ahora = DateTime.UtcNow;
        var respuestas = new[]
        {
            RespuestaTrivia.Crear(
                sesionId, alpha.ParticipanteId, preguntaId, 0, ahora,
                fueraDeTiempo: false, esCorrecta: true, puntosOtorgados: 100, tiempoRespuestaMs: 8_000),
            RespuestaTrivia.Crear(
                sesionId, beta.ParticipanteId, preguntaId, 0, ahora,
                fueraDeTiempo: false, esCorrecta: true, puntosOtorgados: 100, tiempoRespuestaMs: 3_000),
        };

        var ranking = RankingService.Calcular(sesion.Participantes, respuestas);

        ranking[0].ParticipanteId.Should().Be(beta.ParticipanteId);
        ranking[0].TiempoAcumuladoMs.Should().Be(3_000);
        ranking[0].Posicion.Should().Be(1);
        ranking[1].ParticipanteId.Should().Be(alpha.ParticipanteId);
        ranking[1].TiempoAcumuladoMs.Should().Be(8_000);
        ranking[1].Posicion.Should().Be(2);
    }

    [Fact]
    public void Calcular_FueraDeTiempo_NoSumaTiempoUtil()
    {
        var (sesion, pregunta, alphaId, betaId) = CrearSesionTriviaConDosParticipantes();
        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);

        sesion.RegistrarRespuestaTrivia(alphaId, pregunta, 0, t0.AddSeconds(5), Duracion);
        sesion.RegistrarRespuestaTrivia(
            betaId, pregunta, 0, t0.AddSeconds(Duracion + 2), Duracion);

        var tiempos = RankingService.CalcularTiemposAcumulados(sesion.RespuestasTrivia);

        tiempos[alphaId].Should().BeGreaterThan(0);
        tiempos.ContainsKey(betaId).Should().BeFalse();

        var ranking = RankingService.Calcular(sesion.Participantes, sesion.RespuestasTrivia);
        ranking.Single(r => r.ParticipanteId == betaId).TiempoAcumuladoMs.Should().Be(0);
        ranking.Single(r => r.ParticipanteId == alphaId).TiempoAcumuladoMs.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Calcular_TiempoIdentico_DesempataPorNombre()
    {
        var sesion = SesionBuilder.BusquedaTesoro()
            .Activa().ConParticipante("Zeta").ConParticipante("Alpha").Build();
        var alpha = sesion.Participantes.First(e => e.Nombre.Valor == "Alpha");
        var zeta  = sesion.Participantes.First(e => e.Nombre.Valor == "Zeta");
        alpha.SumarPuntaje(100);
        zeta.SumarPuntaje(100);

        var sesionId = sesion.SesionId;
        var preguntaId = PreguntaId.Nuevo();
        var ahora = DateTime.UtcNow;
        var respuestas = new[]
        {
            RespuestaTrivia.Crear(
                sesionId, alpha.ParticipanteId, preguntaId, 0, ahora,
                false, true, 100, 5_000),
            RespuestaTrivia.Crear(
                sesionId, zeta.ParticipanteId, preguntaId, 0, ahora,
                false, true, 100, 5_000),
        };

        var ranking = RankingService.Calcular(sesion.Participantes, respuestas);

        ranking[0].NombreParticipante.Should().Be("Alpha");
        ranking[1].NombreParticipante.Should().Be("Zeta");
        ranking[0].TiempoAcumuladoMs.Should().Be(5_000);
        ranking[1].TiempoAcumuladoMs.Should().Be(5_000);
    }

    private static (SesionAR sesion, Pregunta pregunta, ParticipanteId alphaId, ParticipanteId betaId)
        CrearSesionTriviaConDosParticipantes()
    {
        var pregunta = Pregunta.Crear(
            "¿Capital?",
            Dificultad.Facil,
            [
                OpcionRespuesta.Crear("Madrid", true),
                OpcionRespuesta.Crear("Lisboa", false),
                OpcionRespuesta.Crear("París", false)
            ]);

        var snapshot = MisionSnapshot.SoloTrivia([pregunta.PreguntaId], "Geo");
        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        var alpha = SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        var beta = SesionTestHelpers.UnirParticipante(sesion, "Beta");
        sesion.Iniciar();
        return (sesion, pregunta, alpha.ParticipanteId, beta.ParticipanteId);
    }
}
