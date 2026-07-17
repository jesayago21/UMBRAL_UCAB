using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Misiones.Commands.EliminarEtapaMision;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class EliminarEtapaMisionCommandHandlerTests
{
    [Fact]
    public async Task Handle_EliminaEtapaYPersiste()
    {
        var mision = Mision.Crear("Dos etapas");
        mision.AgregarEtapaBusquedaTesoro("A", "QR-A");
        mision.AgregarEtapaBusquedaTesoro("B", "QR-B");
        var etapaId = mision.Etapas[0].EtapaId.Valor;

        var repo = Substitute.For<IMisionRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(false);

        var handler = new EliminarEtapaMisionCommandHandler(repo);

        var result = await handler.Handle(
            new EliminarEtapaMisionCommand(mision.MisionId.Valor, etapaId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        mision.Etapas.Should().ContainSingle();
        ((EtapaBusquedaTesoro)mision.Etapas[0]).Descripcion.Should().Be("B");
        await repo.Received(1).SaveAsync(mision, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoSesionActiva_LanzaDomainException()
    {
        var mision = Mision.Crear("En uso");
        mision.AgregarEtapaBusquedaTesoro("A", "QR-A");

        var repo = Substitute.For<IMisionRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>()).Returns(true);

        var handler = new EliminarEtapaMisionCommandHandler(repo);

        var act = () => handler.Handle(
            new EliminarEtapaMisionCommand(mision.MisionId.Valor, mision.Etapas[0].EtapaId.Valor),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*sesiones activas*");
    }
}
