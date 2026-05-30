using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.ActualizarPregunta;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Preguntas.Commands;

/// <summary>HU-26 — ActualizarPregunta (Application).</summary>
public sealed class ActualizarPreguntaCommandHandlerTests
{
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly ICategoriaRepository _categoriaRepo = Substitute.For<ICategoriaRepository>();
    private readonly ActualizarPreguntaCommandHandler _sut;

    public ActualizarPreguntaCommandHandlerTests()
        => _sut = new ActualizarPreguntaCommandHandler(_preguntaRepo, _categoriaRepo);

    [Fact]
    public async Task Handle_CuandoExiste_ModificaContenidoYPersiste()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();

        _preguntaRepo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns(pregunta);

        var command = new ActualizarPreguntaCommand(
            pregunta.PreguntaId.Valor,
            "Nuevo enunciado",
            "Dificil",
            null,
            [
                new OpcionRespuestaInput("X", true),
                new OpcionRespuestaInput("Y", false),
                new OpcionRespuestaInput("Z", false)
            ]);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pregunta.Enunciado.Should().Be("Nuevo enunciado");
        pregunta.Dificultad.Should().Be(Dificultad.Dificil);

        await _preguntaRepo.Received(1).SaveAsync(pregunta, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoNoExiste_LanzaNotFoundException()
    {
        _preguntaRepo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns((Pregunta?)null);

        var command = new ActualizarPreguntaCommand(
            Guid.NewGuid(),
            "Enunciado",
            "Facil",
            null,
            [
                new OpcionRespuestaInput("A", true),
                new OpcionRespuestaInput("B", false),
                new OpcionRespuestaInput("C", false)
            ]);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoPreguntaEliminada_LanzaNotFoundException()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        pregunta.Eliminar();

        _preguntaRepo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns(pregunta);

        var command = ComandoConCategoria(pregunta.PreguntaId.Valor, null);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _preguntaRepo.DidNotReceive().SaveAsync(Arg.Any<Pregunta>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoAsignaCategoriaExistente_PersisteConCategoria()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var categoria = TriviaTestBuilder.UnaCategoria();

        _preguntaRepo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns(pregunta);
        _categoriaRepo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);

        var command = ComandoConCategoria(pregunta.PreguntaId.Valor, categoria.CategoriaId.Valor);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pregunta.CategoriaId.Should().Be(categoria.CategoriaId);
        await _preguntaRepo.Received(1).SaveAsync(pregunta, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoCategoriaNoExiste_LanzaNotFoundException()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();

        _preguntaRepo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns(pregunta);
        _categoriaRepo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns((Categoria?)null);

        var command = ComandoConCategoria(pregunta.PreguntaId.Valor, Guid.NewGuid());

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoCategoriaEliminada_LanzaNotFoundException()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var categoria = TriviaTestBuilder.UnaCategoria();
        categoria.Eliminar();

        _preguntaRepo.FindByIdAsync(Arg.Any<PreguntaId>(), Arg.Any<CancellationToken>())
            .Returns(pregunta);
        _categoriaRepo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);

        var command = ComandoConCategoria(pregunta.PreguntaId.Valor, categoria.CategoriaId.Valor);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static ActualizarPreguntaCommand ComandoConCategoria(Guid preguntaId, Guid? categoriaId)
        => new(
            preguntaId,
            "Enunciado actualizado",
            "Media",
            categoriaId,
            [
                new OpcionRespuestaInput("A", true),
                new OpcionRespuestaInput("B", false),
                new OpcionRespuestaInput("C", false)
            ]);
}
