using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.LiberarPistaManual;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>RF-15 — LiberarPistaManual.</summary>
public sealed class LiberarPistaManualCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly LiberarPistaManualCommandHandler _sut;

    public LiberarPistaManualCommandHandlerTests()
    {
        _sut = new LiberarPistaManualCommandHandler(_sesionRepo, _publisher, _notifier);
    }

    [Fact]
    public async Task Handle_CuandoActivaYTodos_PersistePublicaYNotifica()
    {
        var sesion = CrearSesionActivaDosParticipantes();

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);
        _sesionRepo
            .SaveAsync(sesion, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _notifier
            .NotificarPistaLiberadaAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new LiberarPistaManualCommand(sesion.SesionId.Valor, "Ayuda situacional", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        await _sesionRepo.Received(1).SaveAsync(sesion, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishBatchAsync(
            Arg.Any<IReadOnlyList<IDomainEvent>>(),
            Arg.Any<CancellationToken>());
        await _notifier.Received(2).NotificarPistaLiberadaAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<int>(), "Ayuda situacional", Arg.Any<CancellationToken>());
        sesion.ContextoMision!.PistasEntregadas.Should().HaveCount(2);
        sesion.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new LiberarPistaManualCommand(Guid.NewGuid(), "Texto", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_AUnParticipante_NotificaSoloAEse()
    {
        var sesion = CrearSesionActivaDosParticipantes();
        var alpha = sesion.Participantes[0];

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);
        _sesionRepo
            .SaveAsync(sesion, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _notifier
            .NotificarPistaLiberadaAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new LiberarPistaManualCommand(
                sesion.SesionId.Valor,
                "Solo Alpha",
                alpha.ParticipanteId.Valor),
            CancellationToken.None);

        result.Value.Should().Be(1);
        await _notifier.Received(1).NotificarPistaLiberadaAsync(
            sesion.SesionId.Valor.ToString(),
            alpha.ParticipanteId.Valor.ToString(),
            Arg.Any<string>(),
            0,
            "Solo Alpha",
            Arg.Any<CancellationToken>());
    }

    private static SesionAR CrearSesionActivaDosParticipantes()
    {
        var mision = Mision.Crear("RF-15 handler");
        mision.AgregarEtapaBusquedaTesoro("Etapa", "QR-H15");
        mision.Activar();
        mision.ClearDomainEvents();

        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(mision),
            UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        sesion.UnirseParticipante(UsuarioId.Nuevo(), "Alpha", sesion.CodigoAcceso.Valor);
        sesion.UnirseParticipante(UsuarioId.Nuevo(), "Beta", sesion.CodigoAcceso.Valor);
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }
}
