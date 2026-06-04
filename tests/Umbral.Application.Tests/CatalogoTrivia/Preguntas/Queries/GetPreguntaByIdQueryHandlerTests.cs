using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Preguntas.Queries.GetPreguntaById;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Preguntas.Queries;

public sealed class GetPreguntaByIdQueryHandlerTests
{
    private readonly IPreguntaRepository _repo = Substitute.For<IPreguntaRepository>();
    private readonly GetPreguntaByIdQueryHandler _sut;

    public GetPreguntaByIdQueryHandlerTests()
        => _sut = new GetPreguntaByIdQueryHandler(_repo);

    [Fact]
    public async Task Handle_CuandoExiste_RetornaDtoConOpciones()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();

        _repo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns(pregunta);

        var dto = await _sut.Handle(
            new GetPreguntaByIdQuery(pregunta.PreguntaId.Valor),
            CancellationToken.None);

        dto.Id.Should().Be(pregunta.PreguntaId.Valor);
        dto.Enunciado.Should().Be(pregunta.Enunciado);
        dto.Opciones.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_CuandoEliminada_LanzaNotFoundException()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        pregunta.Eliminar();

        _repo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns(pregunta);

        var act = () => _sut.Handle(
            new GetPreguntaByIdQuery(pregunta.PreguntaId.Valor),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
