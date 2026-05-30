using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.EliminarCategoria;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Categorias.Commands;

/// <summary>HU-31 — EliminarCategoria (Application). RB-14.</summary>
public sealed class EliminarCategoriaCommandHandlerTests
{
    private readonly ICategoriaRepository _categoriaRepo = Substitute.For<ICategoriaRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly EliminarCategoriaCommandHandler _sut;

    public EliminarCategoriaCommandHandlerTests()
        => _sut = new EliminarCategoriaCommandHandler(_categoriaRepo, _preguntaRepo);

    [Fact]
    public async Task Handle_CuandoExiste_EliminaCategoriaYQuitaCategoriaDePreguntas()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var pregunta = TriviaTestBuilder.PreguntaConCategoria(categoria.CategoriaId);

        _categoriaRepo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);
        _preguntaRepo.FindByCategoriaAsync(categoria.CategoriaId, Arg.Any<CancellationToken>())
            .Returns([pregunta]);

        var result = await _sut.Handle(
            new EliminarCategoriaCommand(categoria.CategoriaId.Valor),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        categoria.Eliminada.Should().BeTrue();
        pregunta.CategoriaId.Should().BeNull();

        await _preguntaRepo.Received(1).SaveAsync(pregunta, Arg.Any<CancellationToken>());
        await _categoriaRepo.Received(1).SaveAsync(categoria, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoNoExiste_LanzaNotFoundException()
    {
        _categoriaRepo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns((Categoria?)null);

        var act = () => _sut.Handle(
            new EliminarCategoriaCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
