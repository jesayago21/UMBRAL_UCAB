using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.CancelarSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class CancelarSesionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly CancelarSesionCommandHandler _sut;

    public CancelarSesionCommandHandlerTests()
    {
        _sut = new CancelarSesionCommandHandler(_sesionRepo, _publisher, _notifier);
    }

    [Fact]
    public async Task Handle_CuandoSesionCancelable_CancelaYPublicaEvento()
    {
        // Arrange
        var sesion = SesionTestBuilder.EnPreparacionSinParticipantes();
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);
        _sesionRepo.SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        IReadOnlyList<IDomainEvent>? eventos = null;
        _publisher.PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                eventos = ci.ArgAt<IReadOnlyList<IDomainEvent>>(0).ToList();
                return Task.CompletedTask;
            });

        // Act
        var result = await _sut.Handle(
            new CancelarSesionCommand(sesion.SesionId.Valor, "Cancelación operativa"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sesion.Estado.Should().Be(EstadoSesion.Cancelada);
        eventos.Should().ContainSingle(e => e is SesionCancelada);
        await _notifier.Received(1).NotificarCambioEstadoSesionAsync(
            sesion.SesionId.Valor.ToString(),
            "Cancelada",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns((SesionAR?)null);

        // Act
        var act = () => _sut.Handle(
            new CancelarSesionCommand(Guid.NewGuid(), "Motivo"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoSesionYaFinalizada_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionTestBuilder.Finalizada();
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        // Act
        var act = () => _sut.Handle(
            new CancelarSesionCommand(sesion.SesionId.Valor, "No aplica"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}
