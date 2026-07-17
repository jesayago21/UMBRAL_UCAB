using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

public sealed class ValidacionRespuestaTriviaServiceTests
{
    [Fact]
    public void EsFueraDeTiempo_CuandoTimestampPosteriorAlCierre_RetornaTrue()
    {
        var cierre = DateTime.Parse("2026-07-14T18:00:30Z").ToUniversalTime();
        var timestamp = cierre.AddMilliseconds(200);

        ValidacionRespuestaTriviaService.EsFueraDeTiempo(timestamp, cierre).Should().BeTrue();
    }

    [Fact]
    public void EsFueraDeTiempo_CuandoTimestampAnteriorAlCierre_RetornaFalse()
    {
        var cierre = DateTime.Parse("2026-07-14T18:00:30Z").ToUniversalTime();
        var timestamp = cierre.AddMilliseconds(-100);

        ValidacionRespuestaTriviaService.EsFueraDeTiempo(timestamp, cierre).Should().BeFalse();
    }

    [Fact]
    public void EsFueraDeTiempo_SinTimer_RetornaTrue()
    {
        ValidacionRespuestaTriviaService
            .EsFueraDeTiempo(DateTime.UtcNow, null)
            .Should().BeTrue();
    }
}

public sealed class CalculoPuntajeTriviaServiceTests
{
    [Fact]
    public void Calcular_Incorrecta_RetornaCero()
    {
        CalculoPuntajeTriviaService.Calcular(false, false, 1000, 30_000).Valor.Should().Be(0);
    }

    [Fact]
    public void Calcular_FueraDeTiempo_RetornaCero()
    {
        CalculoPuntajeTriviaService.Calcular(true, true, 30_000, 30_000).Valor.Should().Be(0);
    }

    [Fact]
    public void Calcular_CorrectaRapida_OtorgaBaseMasBonus()
    {
        var puntos = CalculoPuntajeTriviaService.Calcular(true, false, 0, 30_000);
        puntos.Valor.Should().Be(150);
    }
}

public sealed class SesionRespuestaTriviaTests
{
    private const int Duracion = 30;

    [Fact]
    public void RegistrarRespuestaTrivia_CorrectaATiempo_SumaPuntos()
    {
        var (sesion, pregunta, participanteId) = CrearSesionConPreguntaActiva();
        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);

        var resp = sesion.RegistrarRespuestaTrivia(
            participanteId,
            pregunta,
            indiceOpcion: 0,
            t0.AddSeconds(5),
            Duracion);

        resp.EsCorrecta.Should().BeTrue();
        resp.FueraDeTiempo.Should().BeFalse();
        resp.PuntosOtorgados.Should().BeGreaterThan(0);
        sesion.Participantes.Single().PuntajeTotal.Valor.Should().Be(resp.PuntosOtorgados);
    }

    [Fact]
    public void RegistrarRespuestaTrivia_FueraDeTiempo_CeroPuntos()
    {
        var (sesion, pregunta, participanteId) = CrearSesionConPreguntaActiva();
        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);

        var resp = sesion.RegistrarRespuestaTrivia(
            participanteId,
            pregunta,
            indiceOpcion: 0,
            t0.AddSeconds(Duracion + 1),
            Duracion);

        resp.FueraDeTiempo.Should().BeTrue();
        resp.PuntosOtorgados.Should().Be(0);
        sesion.Participantes.Single().PuntajeTotal.Valor.Should().Be(0);
    }

    [Fact]
    public void RegistrarRespuestaTrivia_Incorrecta_CeroPuntos()
    {
        var (sesion, pregunta, participanteId) = CrearSesionConPreguntaActiva();
        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);

        var resp = sesion.RegistrarRespuestaTrivia(
            participanteId,
            pregunta,
            indiceOpcion: 1,
            t0.AddSeconds(2),
            Duracion);

        resp.EsCorrecta.Should().BeFalse();
        resp.PuntosOtorgados.Should().Be(0);
    }

    [Fact]
    public void RegistrarRespuestaTrivia_SegundaVez_LanzaDomainException()
    {
        var (sesion, pregunta, participanteId) = CrearSesionConPreguntaActiva();
        var t0 = DateTimeOffset.UtcNow;
        sesion.IniciarSecuenciaTrivia(t0, Duracion);
        sesion.RegistrarRespuestaTrivia(participanteId, pregunta, 0, t0.AddSeconds(1), Duracion);

        var act = () => sesion.RegistrarRespuestaTrivia(
            participanteId, pregunta, 1, t0.AddSeconds(2), Duracion);

        act.Should().Throw<DomainException>().WithMessage("*modificar*");
    }

    private static (SesionAR sesion, Pregunta pregunta, ParticipanteId participanteId)
        CrearSesionConPreguntaActiva()
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
        var p = SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        sesion.Iniciar();
        return (sesion, pregunta, p.ParticipanteId);
    }
}
