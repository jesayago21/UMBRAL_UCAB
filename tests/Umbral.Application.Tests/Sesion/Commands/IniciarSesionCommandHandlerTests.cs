using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.IniciarSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-14 — IniciarSesion. RB-18.</summary>
public sealed class IniciarSesionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly IniciarSesionCommandHandler _sut;

    public IniciarSesionCommandHandlerTests()
    {
        _sut = new IniciarSesionCommandHandler(_sesionRepo, _publisher);
    }

    [Fact]
    public async Task Handle_CuandoEnPreparacionConParticipantes_IniciaYPublicaEvento()
    {
        var sesion = SesionTestBuilder.ConParticipante("Alpha");
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
            new IniciarSesionCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesion.Estado.Should().Be(EstadoSesion.Activa);
        sesion.IniciadaEn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        eventos.Should().ContainSingle().Which.Should().BeOfType<SesionIniciada>();
        sesion.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new IniciarSesionCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoSinParticipantes_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.EnPreparacionSinParticipantes();
        ConfigurarSesion(sesion);

        var act = () => _sut.Handle(
            new IniciarSesionCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*al menos un participante*");
    }

    [Fact]
    public async Task Handle_CuandoNoEstaEnPreparacion_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.Activa();
        ConfigurarSesion(sesion);

        var act = () => _sut.Handle(
            new IniciarSesionCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    private void ConfigurarSesion(SesionAR sesion)
    {
        _sesionRepo
            .FindByIdAsync(
                Arg.Is<SesionId>(id => id.Valor == sesion.SesionId.Valor),
                Arg.Any<CancellationToken>())
            .Returns(sesion);

        _sesionRepo
            .SaveAsync(sesion, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }
}
