using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Categorias.Queries.ListCategorias;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Categorias.Queries;

public sealed class ListCategoriasQueryHandlerTests
{
    private readonly ICategoriaRepository _repo = Substitute.For<ICategoriaRepository>();
    private readonly ListCategoriasQueryHandler _sut;

    public ListCategoriasQueryHandlerTests()
        => _sut = new ListCategoriasQueryHandler(_repo);

    [Fact]
    public async Task Handle_ExcluyeEliminadasYFiltraPorNombre()
    {
        var activa = TriviaTestBuilder.UnaCategoria("Categoría Historia de prueba");
        var otra = TriviaTestBuilder.UnaCategoria("Categoría Ciencia de prueba");
        var eliminada = TriviaTestBuilder.UnaCategoria("Categoría Arte de prueba");
        eliminada.Eliminar();

        _repo.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns([activa, otra, eliminada]);

        var result = await _sut.Handle(
            new ListCategoriasQuery("hist"),
            CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Nombre.Should().Be("Categoría Historia de prueba");
    }
}
