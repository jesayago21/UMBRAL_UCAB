using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Queries.GetEstadoTriviaSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Queries;

public sealed class GetEstadoTriviaSesionQueryHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly GetEstadoTriviaSesionQueryHandler _sut;

    public GetEstadoTriviaSesionQueryHandlerTests() =>
        _sut = new GetEstadoTriviaSesionQueryHandler(_sesionRepo, _preguntaRepo);

    [Fact]
    public async Task Handle_PreguntaActiva_RetornaEstadoCompleto()
    {
        var jugadorId = Guid.NewGuid();
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var sesion = CrearSesionTriviaInscrita(jugadorId, pregunta);
        sesion.IniciarSecuenciaTrivia(DateTimeOffset.UtcNow, 30);

        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);
        _preguntaRepo.FindByIdAsync(pregunta.PreguntaId, Arg.Any<CancellationToken>()).Returns(pregunta);

        var dto = await _sut.Handle(
            new GetEstadoTriviaSesionQuery(sesion.SesionId.Valor, jugadorId),
            CancellationToken.None);

        dto.Fase.Should().Be("PreguntaActiva");
        dto.PreguntaId.Should().Be(pregunta.PreguntaId.Valor);
        dto.Enunciado.Should().Be(pregunta.Enunciado);
        dto.Opciones.Should().HaveCount(3);
        dto.TimerCerradoEnUtc.Should().NotBeNull();
        dto.YaRespondio.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Transicion_RetornaTransicionHasta()
    {
        var jugadorId = Guid.NewGuid();
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var sesion = CrearSesionTriviaInscrita(jugadorId, pregunta);
        sesion.IniciarSecuenciaTrivia(DateTimeOffset.UtcNow, 30);
        sesion.CerrarPreguntaTriviaYEntrarTransicion();

        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var dto = await _sut.Handle(
            new GetEstadoTriviaSesionQuery(sesion.SesionId.Valor, jugadorId),
            CancellationToken.None);

        dto.Fase.Should().Be("Transicion");
        dto.TransicionHastaUtc.Should().NotBeNull();
        dto.Enunciado.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SinInscripcion_LanzaUnauthorized()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var sesion = SesionTestBuilder.ActivaTrivia([pregunta.PreguntaId]);
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var act = () => _sut.Handle(
            new GetEstadoTriviaSesionQuery(sesion.SesionId.Valor, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_SesionInexistente_LanzaNotFound()
    {
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new GetEstadoTriviaSesionQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static SesionAR CrearSesionTriviaInscrita(Guid jugadorId, Pregunta pregunta)
    {
        var snapshot = MisionSnapshot.SoloTrivia([pregunta.PreguntaId], "Trivia test");
        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        sesion.UnirseParticipante(new UsuarioId(jugadorId), "Alpha", sesion.CodigoAcceso.Valor);
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }
}
