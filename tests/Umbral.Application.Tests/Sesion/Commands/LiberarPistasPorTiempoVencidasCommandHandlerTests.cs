using FluentAssertions;
using NSubstitute;
using Umbral.Application.Sesion.Commands.LiberarPistasPorTiempoVencidas;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-09 — LiberarPistasPorTiempoVencidas.</summary>
public sealed class LiberarPistasPorTiempoVencidasCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly LiberarPistasPorTiempoVencidasCommandHandler _sut;

    public LiberarPistasPorTiempoVencidasCommandHandlerTests()
    {
        _sut = new LiberarPistasPorTiempoVencidasCommandHandler(
            _sesionRepo, _publisher, _notifier);
    }

    [Fact]
    public async Task Handle_CuandoPistaVencida_PersistePublicaYNotifica()
    {
        var sesion = CrearSesionActivaConPistaPorTiempo(segundos: 10);
        var ahora = DateTimeOffset.UtcNow.AddSeconds(20);

        _sesionRepo.FindActivasAsync(Arg.Any<CancellationToken>())
            .Returns(new List<SesionAR> { sesion });
        _sesionRepo.SaveAsync(sesion, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _publisher.PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _notifier.NotificarPistaLiberadaAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new LiberarPistasPorTiempoVencidasCommand(ahora),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        await _sesionRepo.Received(1).SaveAsync(sesion, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishBatchAsync(
            Arg.Any<IReadOnlyList<IDomainEvent>>(),
            Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarPistaLiberadaAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        sesion.ContextoMision!.PistasEntregadas.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_SinSesionesActivas_RetornaCero()
    {
        _sesionRepo.FindActivasAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<SesionAR>());

        var result = await _sut.Handle(
            new LiberarPistasPorTiempoVencidasCommand(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        await _sesionRepo.DidNotReceive().SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoTiempoNoVencido_NoPersisteNiNotifica()
    {
        var sesion = CrearSesionActivaConPistaPorTiempo(segundos: 120);
        var ahora = DateTimeOffset.UtcNow.AddSeconds(5);

        _sesionRepo.FindActivasAsync(Arg.Any<CancellationToken>())
            .Returns(new List<SesionAR> { sesion });

        var result = await _sut.Handle(
            new LiberarPistasPorTiempoVencidasCommand(ahora),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
        await _sesionRepo.DidNotReceive().SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishBatchAsync(
            Arg.Any<IReadOnlyList<IDomainEvent>>(),
            Arg.Any<CancellationToken>());
        await _notifier.DidNotReceive().NotificarPistaLiberadaAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static SesionAR CrearSesionActivaConPistaPorTiempo(int segundos)
    {
        var mision = Mision.Crear("HU-09 handler");
        mision.AgregarEtapaBusquedaTesoro("Etapa", "QR-H09");
        ((EtapaBusquedaTesoro)mision.Etapas[0])
            .AgregarPista("Ayuda", TipoLiberacion.PorTiempo, segundos);
        mision.Activar();
        mision.ClearDomainEvents();

        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(mision),
            UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        sesion.UnirseParticipante(UsuarioId.Nuevo(), "Alpha", sesion.CodigoAcceso.Valor);
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }
}
