using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.ReanudarSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-15 — ReanudarSesion.</summary>
public sealed class ReanudarSesionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly ReanudarSesionCommandHandler _sut;

    public ReanudarSesionCommandHandlerTests()
    {
        _sut = new ReanudarSesionCommandHandler(_sesionRepo, _publisher, _notifier);
    }

    [Fact]
    public async Task Handle_CuandoEstaPausada_ReanudaYPublicaEvento()
    {
        var sesion = SesionTestBuilder.Pausada();
        ConfigurarSesion(sesion);

        IReadOnlyList<IDomainEvent>? eventos = null;
        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                eventos = ci.ArgAt<IReadOnlyList<IDomainEvent>>(0).ToList();
                return Task.CompletedTask;
            });

        var result = await _sut.Handle(
            new ReanudarSesionCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesion.Estado.Should().Be(EstadoSesion.Activa);
        eventos.Should().ContainSingle().Which.Should().BeOfType<SesionReanudada>();
        await _notifier.Received(1).NotificarCambioEstadoSesionAsync(
            sesion.SesionId.Valor.ToString(),
            "Activa",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new ReanudarSesionCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoNoEstaPausada_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.Activa();
        ConfigurarSesion(sesion);

        var act = () => _sut.Handle(
            new ReanudarSesionCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    private void ConfigurarSesion(SesionAR sesion)
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        _sesionRepo
            .SaveAsync(sesion, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }
}
