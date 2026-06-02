using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
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
    public void CrearDesdeMision_SoloTrivia_RetornaSesionProgramada()
    {
        var operadorId = UsuarioId.Nuevo();
        var preguntas = new List<PreguntaId>
        {
            PreguntaId.Nuevo(),
            PreguntaId.Nuevo()
        };
        var snapshot = MisionSnapshot.SoloTrivia(preguntas, "Historia, Ciencia");

        var sesion = SesionAR.CrearDesdeMision(snapshot, operadorId);

        sesion.Estado.Should().Be(EstadoSesion.Programada);
        sesion.TipoSesion.Should().Be(TipoSesion.Mision);
        sesion.ContextoMision.Should().NotBeNull();
        sesion.ContextoMision!.ObtenerEtapaActual().Should().BeOfType<EtapaTriviaSnapshot>();
        var trivia = (EtapaTriviaSnapshot)sesion.ContextoMision.ObtenerEtapaActual();
        trivia.PreguntasOrdenadas.Should().HaveCount(2);
        trivia.CategoriasTitulo.Should().Be("Historia, Ciencia");
        sesion.ContextoTrivia.Should().BeNull();
        sesion.ContextoBT.Should().BeNull();
        sesion.CodigoAcceso.Valor.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CrearDesdeMision_SoloTrivia_EmiteSesionCreada()
    {
        var operadorId = UsuarioId.Nuevo();
        var preguntas = new List<PreguntaId> { PreguntaId.Nuevo() };
        var snapshot = MisionSnapshot.SoloTrivia(preguntas, "Historia, Ciencia");

        var sesion = SesionAR.CrearDesdeMision(snapshot, operadorId);

        var evento = sesion.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SesionCreada>().Subject;

        evento.TipoSesion.Should().Be(TipoSesion.Mision);
        evento.OperadorId.Should().Be(operadorId);
    }

    [Fact]
    public void SoloTrivia_SinPreguntas_LanzaDomainException()
    {
        var act = () => MisionSnapshot.SoloTrivia([], "Vacía");

        act.Should().Throw<DomainException>();
    }
}
