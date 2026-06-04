using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.ActualizarCategoria;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Categorias.Commands;

/// <summary>HU-30 — ActualizarCategoria (Application). RB-15.</summary>
public sealed class ActualizarCategoriaCommandHandlerTests
{
    private readonly ICategoriaRepository _repo = Substitute.For<ICategoriaRepository>();
    private readonly ActualizarCategoriaCommandHandler _sut;

    public ActualizarCategoriaCommandHandlerTests()
        => _sut = new ActualizarCategoriaCommandHandler(_repo);

    [Fact]
    public async Task Handle_CuandoExisteYNombreUnico_RenombraYPersiste()
    {
        var categoria = TriviaTestBuilder.UnaCategoria("Categoría Historia de prueba");

        _repo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);
        _repo.ExistsByNombreAsync("Geografía", categoria.CategoriaId.Valor, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.Handle(
            new ActualizarCategoriaCommand(categoria.CategoriaId.Valor, "Geografía"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        categoria.Nombre.Should().Be("Geografía");

        await _repo.Received(1).SaveAsync(categoria, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoNoExiste_LanzaNotFoundException()
    {
        _repo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns((Categoria?)null);

        var act = () => _sut.Handle(
            new ActualizarCategoriaCommand(Guid.NewGuid(), "Nueva"),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoNombreDuplicado_LanzaDomainException()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();

        _repo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);
        _repo.ExistsByNombreAsync("Categoría Ciencia de prueba", categoria.CategoriaId.Valor, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _sut.Handle(
            new ActualizarCategoriaCommand(categoria.CategoriaId.Valor, "Categoría Ciencia de prueba"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Ya existe*");
    }
}
