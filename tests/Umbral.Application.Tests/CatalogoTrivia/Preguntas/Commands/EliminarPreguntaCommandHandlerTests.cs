using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.EliminarPregunta;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Preguntas.Commands;

/// <summary>HU-27 — EliminarPregunta (Application).</summary>
public sealed class EliminarPreguntaCommandHandlerTests
{
    private readonly IPreguntaRepository _repo = Substitute.For<IPreguntaRepository>();
    private readonly EliminarPreguntaCommandHandler _sut;

    public EliminarPreguntaCommandHandlerTests()
        => _sut = new EliminarPreguntaCommandHandler(_repo);

    [Fact]
    public async Task Handle_CuandoExiste_AplicaSoftDelete()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();

        _repo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns(pregunta);

        var result = await _sut.Handle(
            new EliminarPreguntaCommand(pregunta.PreguntaId.Valor),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pregunta.Eliminada.Should().BeTrue();

        await _repo.Received(1).SaveAsync(pregunta, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoNoExiste_LanzaNotFoundException()
    {
        _repo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns((Pregunta?)null);

        var act = () => _sut.Handle(
            new EliminarPreguntaCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
