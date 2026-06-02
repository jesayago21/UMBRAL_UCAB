using FluentAssertions;
using NSubstitute;
using Umbral.Application.Misiones.Commands.AgregarPistaEtapa;
using Umbral.Domain.CatalogoMision.Mision;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class AgregarPistaEtapaCommandHandlerTests
{
    [Fact]
    public async Task Handle_CuandoMisionYEtapaExisten_AgregaPistaYPersiste()
    {
        var mision = Mision.Crear("Misión prueba");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        var etapaId = mision.Etapas[0].EtapaId.Valor;

        var repo = Substitute.For<IMisionRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns(mision);

        var handler = new AgregarPistaEtapaCommandHandler(repo);

        var result = await handler.Handle(
            new AgregarPistaEtapaCommand(
                mision.MisionId.Valor,
                etapaId,
                "Busca cerca del mural",
                "PorTiempo",
                120),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ((EtapaBusquedaTesoro)mision.Etapas[0]).Pistas
            .Should().ContainSingle(p => p.Contenido == "Busca cerca del mural");
        await repo.Received(1).SaveAsync(mision, Arg.Any<CancellationToken>());
    }
}
