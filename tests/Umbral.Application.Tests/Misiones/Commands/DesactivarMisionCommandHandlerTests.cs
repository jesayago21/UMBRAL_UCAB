using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Misiones.Commands.DesactivarMision;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class DesactivarMisionCommandHandlerTests
{
    [Fact]
    public async Task Handle_CuandoMisionActiva_DesactivaYPersiste()
    {
        var mision = MisionTestBuilder.Activa();
        var repo = Substitute.For<IMisionRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);

        var sut = new DesactivarMisionCommandHandler(repo);
        var result = await sut.Handle(
            new DesactivarMisionCommand(mision.MisionId.Valor),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(mision.MisionId.Valor);
        mision.Estado.Should().Be(EstadoMision.Borrador);
        await repo.Received(1).SaveAsync(mision, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoMisionNoExiste_LanzaNotFoundException()
    {
        var repo = Substitute.For<IMisionRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns((Mision?)null);

        var sut = new DesactivarMisionCommandHandler(repo);
        var act = () => sut.Handle(
            new DesactivarMisionCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
