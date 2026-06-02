using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.AplicarPenalizacion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-16 — AplicarPenalizacion (Application).</summary>
public sealed class AplicarPenalizacionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly AplicarPenalizacionCommandHandler _sut;

    public AplicarPenalizacionCommandHandlerTests()
    {
        _sut = new AplicarPenalizacionCommandHandler(_sesionRepo, _publisher);
    }

    [Fact]
    public async Task Handle_CuandoSesionActiva_AplicaPenalizacionYPublicaEvento()
    {
        var sesion = SesionTestBuilder.Activa("Alpha");
        var participante = sesion.Participantes.First();
        participante.SumarPuntaje(100);
        sesion.ClearDomainEvents();

        _sesionRepo
            .FindByIdAsync(Arg.Is<SesionId>(id => id.Valor == sesion.SesionId.Valor), Arg.Any<CancellationToken>())
            .Returns(sesion);
        _sesionRepo
            .SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        IReadOnlyList<IDomainEvent>? eventos = null;
        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                eventos = ci.ArgAt<IReadOnlyList<IDomainEvent>>(0).ToList();
                return Task.CompletedTask;
            });

        var result = await _sut.Handle(
            new AplicarPenalizacionCommand(
                sesion.SesionId.Valor,
                participante.ParticipanteId.Valor,
                20,
                "Trampa detectada",
                Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        participante.PuntajeTotal.Valor.Should().Be(80);
        eventos.Should().ContainSingle().Which.Should().BeOfType<PenalizacionAplicada>();
        sesion.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new AplicarPenalizacionCommand(Guid.NewGuid(), Guid.NewGuid(), 10, "Motivo", Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoSesionNoActiva_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.ConParticipante("Alpha");
        var participante = sesion.Participantes.First();

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var act = () => _sut.Handle(
            new AplicarPenalizacionCommand(
                sesion.SesionId.Valor,
                participante.ParticipanteId.Valor,
                5,
                "Fuera de reglas",
                Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*sesiones activas*");
    }
}
