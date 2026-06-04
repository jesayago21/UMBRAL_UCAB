using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.PausarSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-15 — PausarSesion.</summary>
public sealed class PausarSesionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly PausarSesionCommandHandler _sut;

    public PausarSesionCommandHandlerTests()
    {
        _sut = new PausarSesionCommandHandler(_sesionRepo, _publisher);
    }

    [Fact]
    public async Task Handle_CuandoEstaActiva_PausaYPublicaEvento()
    {
        var sesion = SesionTestBuilder.Activa();
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
            new PausarSesionCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesion.Estado.Should().Be(EstadoSesion.Pausada);
        eventos.Should().ContainSingle().Which.Should().BeOfType<SesionPausada>();
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new PausarSesionCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoNoEstaActiva_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.ConParticipante("Alpha");
        ConfigurarSesion(sesion);

        var act = () => _sut.Handle(
            new PausarSesionCommand(sesion.SesionId.Valor),
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
