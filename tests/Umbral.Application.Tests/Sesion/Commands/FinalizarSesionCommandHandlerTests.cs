using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.FinalizarSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class FinalizarSesionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly FinalizarSesionCommandHandler _sut;

    public FinalizarSesionCommandHandlerTests()
    {
        _sut = new FinalizarSesionCommandHandler(_sesionRepo, _publisher);
    }

    [Fact]
    public async Task Handle_CuandoSesionActiva_FinalizaYPublicaEvento()
    {
        // Arrange
        var sesion = SesionTestBuilder.Activa("Alpha");
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
        var result = await _sut.Handle(new FinalizarSesionCommand(sesion.SesionId.Valor), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
        eventos.Should().ContainSingle(e => e is SesionFinalizada);
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns((SesionAR?)null);

        // Act
        var act = () => _sut.Handle(new FinalizarSesionCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoEstadoNoPermiteFinalizar_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionTestBuilder.EnPreparacionSinEquipos();
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        // Act
        var act = () => _sut.Handle(new FinalizarSesionCommand(sesion.SesionId.Valor), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}
