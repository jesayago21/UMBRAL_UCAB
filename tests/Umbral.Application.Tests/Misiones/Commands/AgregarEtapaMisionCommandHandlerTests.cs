using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Misiones.Commands.AgregarEtapaMision;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class AgregarEtapaMisionCommandHandlerTests
{
    [Fact]
    public async Task Handle_EtapaBt_AgregaYPersiste()
    {
        var mision = Mision.Crear("Misión etapas");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-1");

        var repo = Substitute.For<IMisionRepository>();
        var categoriaRepo = Substitute.For<ICategoriaRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(false);

        var handler = new AgregarEtapaMisionCommandHandler(repo, categoriaRepo);

        var result = await handler.Handle(
            new AgregarEtapaMisionCommand(
                mision.MisionId.Valor,
                "BusquedaTesoro",
                "Patio",
                "QR-PATIO",
                null,
                [new AgregarEtapaPistaInput("Pista A", "PorTiempo", 30)],
                null,
                null,
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        mision.Etapas.Should().HaveCount(2);
        var bt = (EtapaBusquedaTesoro)mision.Etapas[1];
        bt.Descripcion.Should().Be("Patio");
        bt.Pistas.Should().ContainSingle(p => p.Contenido == "Pista A");
        await repo.Received(1).SaveAsync(mision, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EtapaTrivia_AgregaCategorias()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var mision = Mision.Crear("Misión trivia");

        var repo = Substitute.For<IMisionRepository>();
        var categoriaRepo = Substitute.For<ICategoriaRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(false);
        categoriaRepo.FindByIdAsync(categoria.CategoriaId, Arg.Any<CancellationToken>()).Returns(categoria);

        var handler = new AgregarEtapaMisionCommandHandler(repo, categoriaRepo);

        var result = await handler.Handle(
            new AgregarEtapaMisionCommand(
                mision.MisionId.Valor,
                "Trivia",
                null,
                null,
                [categoria.CategoriaId.Valor],
                null,
                null,
                null,
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        mision.Etapas.Should().ContainSingle(e => e is EtapaTrivia);
        await repo.Received(1).SaveAsync(mision, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EtapaTriviaCategoriaInexistente_LanzaNotFoundException()
    {
        var mision = Mision.Crear("Misión trivia");
        var catId = Guid.NewGuid();

        var repo = Substitute.For<IMisionRepository>();
        var categoriaRepo = Substitute.For<ICategoriaRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(false);
        categoriaRepo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns((Categoria?)null);

        var handler = new AgregarEtapaMisionCommandHandler(repo, categoriaRepo);

        var act = () => handler.Handle(
            new AgregarEtapaMisionCommand(
                mision.MisionId.Valor,
                "Trivia",
                null,
                null,
                [catId],
                null,
                null,
                null,
                null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoSesionActiva_LanzaDomainException()
    {
        var mision = Mision.Crear("En uso");
        var repo = Substitute.For<IMisionRepository>();
        var categoriaRepo = Substitute.For<ICategoriaRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(true);

        var handler = new AgregarEtapaMisionCommandHandler(repo, categoriaRepo);

        var act = () => handler.Handle(
            new AgregarEtapaMisionCommand(
                mision.MisionId.Valor,
                "BusquedaTesoro",
                "X",
                "QR-X",
                null,
                null,
                null,
                null,
                null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*sesiones activas*");
    }
}
