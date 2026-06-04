using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Categorias.Commands.CrearCategoria;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Categorias.Commands;

/// <summary>HU-28 — CrearCategoria (Application). RB-15.</summary>
public sealed class CrearCategoriaCommandHandlerTests
{
    private readonly ICategoriaRepository _repo = Substitute.For<ICategoriaRepository>();
    private readonly CrearCategoriaCommandHandler _sut;

    public CrearCategoriaCommandHandlerTests()
        => _sut = new CrearCategoriaCommandHandler(_repo);

    [Fact]
    public async Task Handle_CuandoNombreUnico_CreaYPersiste()
    {
        _repo.ExistsByNombreAsync("Historia", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.Handle(new CrearCategoriaCommand("  Historia  "), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _repo.Received(1).SaveAsync(
            Arg.Is<Categoria>(c => c.Nombre == "Historia" && !c.Eliminada),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoNombreDuplicado_LanzaDomainException()
    {
        _repo.ExistsByNombreAsync("Historia", null, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _sut.Handle(new CrearCategoriaCommand("Historia"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Ya existe*");

        await _repo.DidNotReceive().SaveAsync(
            Arg.Any<Categoria>(),
            Arg.Any<CancellationToken>());
    }
}
