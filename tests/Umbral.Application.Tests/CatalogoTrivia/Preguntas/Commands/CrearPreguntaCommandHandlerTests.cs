using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Models;
using Umbral.Application.CatalogoTrivia.Preguntas.Commands.CrearPregunta;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Preguntas.Commands;

/// <summary>HU-24 — CrearPregunta (Application).</summary>
public sealed class CrearPreguntaCommandHandlerTests
{
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly ICategoriaRepository _categoriaRepo = Substitute.For<ICategoriaRepository>();
    private readonly CrearPreguntaCommandHandler _sut;

    public CrearPreguntaCommandHandlerTests()
        => _sut = new CrearPreguntaCommandHandler(_preguntaRepo, _categoriaRepo);

    [Fact]
    public async Task Handle_SinCategoria_CreaYPersiste()
    {
        var command = new CrearPreguntaCommand(
            "Enunciado crear pregunta de prueba",
            "Facil",
            null,
            [
                new OpcionRespuestaInput("Opción A de prueba", true),
                new OpcionRespuestaInput("Opción B de prueba", false),
                new OpcionRespuestaInput("Opción C de prueba", false)
            ]);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _preguntaRepo.Received(1).SaveAsync(
            Arg.Is<Pregunta>(p =>
                p.Enunciado == "Enunciado crear pregunta de prueba"
                && p.Dificultad == Dificultad.Facil
                && p.CategoriaId == null
                && !p.Eliminada),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConCategoriaValida_AsignaCategoria()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();

        _categoriaRepo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);

        var command = new CrearPreguntaCommand(
            "Enunciado con categoría de prueba",
            "Media",
            categoria.CategoriaId.Valor,
            [
                new OpcionRespuestaInput("Opción correcta de prueba", true),
                new OpcionRespuestaInput("Opción incorrecta 1 de prueba", false),
                new OpcionRespuestaInput("Opción incorrecta 2 de prueba", false)
            ]);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        await _preguntaRepo.Received(1).SaveAsync(
            Arg.Is<Pregunta>(p => p.CategoriaId == categoria.CategoriaId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoCategoriaNoExiste_LanzaNotFoundException()
    {
        _categoriaRepo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns((Categoria?)null);

        var command = new CrearPreguntaCommand(
            "Pregunta",
            "Facil",
            Guid.NewGuid(),
            [
                new OpcionRespuestaInput("A", true),
                new OpcionRespuestaInput("B", false),
                new OpcionRespuestaInput("C", false)
            ]);

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
