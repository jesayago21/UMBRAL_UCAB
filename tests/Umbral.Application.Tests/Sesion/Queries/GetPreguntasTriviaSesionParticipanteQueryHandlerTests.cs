using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Queries.GetPreguntasTriviaSesionParticipante;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Queries;

public sealed class GetPreguntasTriviaSesionParticipanteQueryHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly GetPreguntasTriviaSesionParticipanteQueryHandler _sut;

    public GetPreguntasTriviaSesionParticipanteQueryHandlerTests()
    {
        _sut = new GetPreguntasTriviaSesionParticipanteQueryHandler(_sesionRepo, _preguntaRepo);
    }

    [Fact]
    public async Task Handle_CuandoParticipanteInscrito_RetornaPreguntasOrdenadas()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var pregunta = TriviaTestBuilder.PreguntaConCategoria(categoria.CategoriaId);
        var operadorId = UsuarioId.Nuevo();
        var jugadorId = UsuarioId.Nuevo();
        var sesion = SesionAR.CrearTrivia(
            [pregunta.PreguntaId],
            operadorId,
            categoria.Nombre);
        sesion.UnirseParticipante(jugadorId, "Alpha", sesion.CodigoAcceso.Valor);

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        _preguntaRepo
            .FindByIdsAsync(Arg.Any<IReadOnlyList<PreguntaId>>(), Arg.Any<CancellationToken>())
            .Returns([pregunta]);

        var result = await _sut.Handle(
            new GetPreguntasTriviaSesionParticipanteQuery(sesion.SesionId.Valor, jugadorId.Valor),
            CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Orden.Should().Be(1);
        result[0].Enunciado.Should().Be(pregunta.Enunciado);
        result[0].Opciones.Should().HaveCountGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task Handle_CuandoNoInscrito_LanzaUnauthorized()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var pregunta = TriviaTestBuilder.PreguntaConCategoria(categoria.CategoriaId);
        var sesion = SesionAR.CrearTrivia(
            [pregunta.PreguntaId],
            UsuarioId.Nuevo(),
            categoria.Nombre);

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var act = () => _sut.Handle(
            new GetPreguntasTriviaSesionParticipanteQuery(
                sesion.SesionId.Valor,
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFound()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new GetPreguntasTriviaSesionParticipanteQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
