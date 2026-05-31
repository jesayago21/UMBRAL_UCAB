using FluentAssertions;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

public sealed class SesionCrearTriviaTests
{
    [Fact]
    public void CrearTrivia_ConPreguntasValidas_RetornaSesionProgramada()
    {
        var operadorId = UsuarioId.Nuevo();
        var preguntas = new List<PreguntaId>
        {
            PreguntaId.Nuevo(),
            PreguntaId.Nuevo()
        };

        var sesion = SesionAR.CrearTrivia(preguntas, operadorId);

        sesion.Estado.Should().Be(EstadoSesion.Programada);
        sesion.TipoSesion.Should().Be(TipoSesion.Trivia);
        sesion.ContextoTrivia.Should().NotBeNull();
        sesion.ContextoTrivia!.TotalPreguntas.Should().Be(2);
        sesion.ContextoBT.Should().BeNull();
        sesion.CodigoAcceso.Valor.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CrearTrivia_ConPreguntasValidas_EmiteSesionCreada()
    {
        var operadorId = UsuarioId.Nuevo();
        var preguntas = new List<PreguntaId> { PreguntaId.Nuevo() };

        var sesion = SesionAR.CrearTrivia(preguntas, operadorId);

        var evento = sesion.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SesionCreada>().Subject;

        evento.TipoSesion.Should().Be(TipoSesion.Trivia);
        evento.OperadorId.Should().Be(operadorId);
    }

    [Fact]
    public void CrearTrivia_SinPreguntas_LanzaDomainException()
    {
        var operadorId = UsuarioId.Nuevo();

        var act = () => SesionAR.CrearTrivia([], operadorId);

        act.Should().Throw<DomainException>();
    }
}
