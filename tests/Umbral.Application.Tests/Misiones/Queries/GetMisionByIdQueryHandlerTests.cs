using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Misiones.Queries.GetMisionById;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Queries;

public sealed class GetMisionByIdQueryHandlerTests
{
    private readonly IMisionRepository _repo = Substitute.For<IMisionRepository>();
    private readonly GetMisionByIdQueryHandler _sut;

    public GetMisionByIdQueryHandlerTests()
        => _sut = new GetMisionByIdQueryHandler(_repo);

    [Fact]
    public async Task Handle_ConMisionBt_RetornaDto()
    {
        var mision = MisionTestBuilder.Activa();
        _repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);

        var dto = await _sut.Handle(new GetMisionByIdQuery(mision.MisionId.Valor), CancellationToken.None);

        dto.Id.Should().Be(mision.MisionId.Valor);
        dto.Etapas.Should().ContainSingle();
        dto.Etapas[0].TipoEtapa.Should().Be("BusquedaTesoro");
    }

    [Fact]
    public async Task Handle_ConEtapaTrivia_RetornaDtoConCategorias()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var mision = MisionTestBuilder.ActivaConEtapaTrivia(categoria.CategoriaId);
        _repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);

        var dto = await _sut.Handle(new GetMisionByIdQuery(mision.MisionId.Valor), CancellationToken.None);

        dto.Etapas.Should().ContainSingle();
        dto.Etapas[0].TipoEtapa.Should().Be("Trivia");
        dto.Etapas[0].CategoriaIds.Should().Contain(categoria.CategoriaId.Valor);
    }

    [Fact]
    public async Task Handle_CuandoNoExiste_LanzaNotFoundException()
    {
        _repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns((Mision?)null);

        var act = () => _sut.Handle(new GetMisionByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
