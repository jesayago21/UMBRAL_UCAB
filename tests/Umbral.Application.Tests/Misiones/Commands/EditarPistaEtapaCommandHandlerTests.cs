using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Misiones.Commands.EditarPistaEtapa;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class EditarPistaEtapaCommandHandlerTests
{
    [Fact]
    public async Task Handle_CuandoMisionYEtapaExisten_EditaPistaYPersiste()
    {
        var mision = Mision.Crear("Misión prueba");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        var etapa = (EtapaBusquedaTesoro)mision.Etapas[0];
        etapa.AgregarPista("Original", TipoLiberacion.PorTiempo, 30);
        var etapaId = etapa.EtapaId.Valor;
        var pistaId = etapa.Pistas[0].PistaId.Valor;

        var repo = Substitute.For<IMisionRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns(mision);
        repo.HasSesionesActivasAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new EditarPistaEtapaCommandHandler(repo);

        var result = await handler.Handle(
            new EditarPistaEtapaCommand(
                mision.MisionId.Valor,
                etapaId,
                pistaId,
                "Actualizada",
                "PorGanador",
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        etapa.Pistas.Should().ContainSingle(p =>
            p.PistaId.Valor == pistaId && p.Contenido == "Actualizada");
        await repo.Received(1).SaveAsync(mision, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoMisionConSesionActiva_LanzaDomainException()
    {
        var misionId = MisionId.Nuevo();
        var repo = Substitute.For<IMisionRepository>();
        repo.FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns(Mision.Crear("En uso"));
        repo.HasSesionesActivasAsync(misionId, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new EditarPistaEtapaCommandHandler(repo);

        var act = () => handler.Handle(
            new EditarPistaEtapaCommand(
                misionId.Valor,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Nueva",
                "PorGanador",
                null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*sesiones activas*");
    }
}
