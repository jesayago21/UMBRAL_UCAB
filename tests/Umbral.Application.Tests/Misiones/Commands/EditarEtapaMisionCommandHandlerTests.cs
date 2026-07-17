using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Misiones.Commands.EditarEtapaMision;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class EditarEtapaMisionCommandHandlerTests
{
    [Fact]
    public async Task Handle_CuandoEtapaBt_ActualizaYPersiste()
    {
        var mision = Mision.Crear("Misión prueba");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        var etapaId = mision.Etapas[0].EtapaId.Valor;

        var repo = Substitute.For<IMisionRepository>();
        var categoriaRepo = Substitute.For<ICategoriaRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(false);

        var handler = new EditarEtapaMisionCommandHandler(repo, categoriaRepo);

        var result = await handler.Handle(
            new EditarEtapaMisionCommand(
                mision.MisionId.Valor,
                etapaId,
                "Patio",
                "QR-PATIO",
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ((EtapaBusquedaTesoro)mision.Etapas[0]).Descripcion.Should().Be("Patio");
        await repo.Received(1).SaveAsync(mision, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoEtapaBtConUbicacion_ActualizaMapa()
    {
        var mision = Mision.Crear("Misión con mapa");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        var etapaId = mision.Etapas[0].EtapaId.Valor;

        var repo = Substitute.For<IMisionRepository>();
        var categoriaRepo = Substitute.For<ICategoriaRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(false);

        var handler = new EditarEtapaMisionCommandHandler(repo, categoriaRepo);

        var result = await handler.Handle(
            new EditarEtapaMisionCommand(
                mision.MisionId.Valor,
                etapaId,
                "Hall",
                "QR-001",
                null,
                10.488,
                -66.847,
                120),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var etapa = (EtapaBusquedaTesoro)mision.Etapas[0];
        etapa.Latitud.Should().Be(10.488);
        etapa.Longitud.Should().Be(-66.847);
        etapa.RadioMetros.Should().Be(120);
    }

    [Fact]
    public async Task Handle_CuandoSesionActiva_LanzaDomainException()
    {
        var mision = Mision.Crear("En uso");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");

        var repo = Substitute.For<IMisionRepository>();
        var categoriaRepo = Substitute.For<ICategoriaRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(true);

        var handler = new EditarEtapaMisionCommandHandler(repo, categoriaRepo);

        var act = () => handler.Handle(
            new EditarEtapaMisionCommand(
                mision.MisionId.Valor,
                mision.Etapas[0].EtapaId.Valor,
                "X",
                "Y",
                null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*sesiones activas*");
    }
}
