using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Categorias.Queries.GetCategoriaById;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Categorias.Queries;

public sealed class GetCategoriaByIdQueryHandlerTests
{
    private readonly ICategoriaRepository _repo = Substitute.For<ICategoriaRepository>();
    private readonly GetCategoriaByIdQueryHandler _sut;

    public GetCategoriaByIdQueryHandlerTests()
        => _sut = new GetCategoriaByIdQueryHandler(_repo);

    [Fact]
    public async Task Handle_CuandoExiste_RetornaDto()
    {
        var categoria = TriviaTestBuilder.UnaCategoria("Arte de prueba");

        _repo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);

        var dto = await _sut.Handle(
            new GetCategoriaByIdQuery(categoria.CategoriaId.Valor),
            CancellationToken.None);

        dto.Id.Should().Be(categoria.CategoriaId.Valor);
        dto.Nombre.Should().Be("Arte de prueba");
    }

    [Fact]
    public async Task Handle_CuandoEliminada_LanzaNotFoundException()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        categoria.Eliminar();

        _repo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);

        var act = () => _sut.Handle(
            new GetCategoriaByIdQuery(categoria.CategoriaId.Valor),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
