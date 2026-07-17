using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.ExpulsarParticipante;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-32 — ExpulsarParticipante (Application).</summary>
public sealed class ExpulsarParticipanteCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly ExpulsarParticipanteCommandHandler _sut;

    public ExpulsarParticipanteCommandHandlerTests() =>
        _sut = new ExpulsarParticipanteCommandHandler(_sesionRepo, _notifier);

    [Fact]
    public async Task Handle_CuandoEnPreparacion_EliminaYNotifica()
    {
        var sesion = SesionTestBuilder.ConParticipante("Alpha");
        var participanteId = sesion.Participantes.Single().ParticipanteId;

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var result = await _sut.Handle(
            new ExpulsarParticipanteCommand(
                sesion.SesionId.Valor,
                participanteId.Valor,
                "Nombre inapropiado"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesion.Participantes.Should().BeEmpty();

        await _sesionRepo.Received(1).EliminarParticipanteAsync(
            participanteId,
            Arg.Any<CancellationToken>());
        await _sesionRepo.Received(1).SaveAsync(sesion, Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarParticipantesActualizadosAsync(
            sesion.SesionId.Valor.ToString(),
            0,
            Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarRankingActualizadoAsync(
            sesion.SesionId.Valor.ToString(),
            Arg.Any<IReadOnlyList<RankingPosicionNotificacion>>(),
            Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarParticipanteExpulsadoAsync(
            sesion.SesionId.Valor.ToString(),
            participanteId.Valor.ToString(),
            "Nombre inapropiado",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new ExpulsarParticipanteCommand(Guid.NewGuid(), Guid.NewGuid(), "Motivo"),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoSesionActiva_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.Activa("Alpha");
        var participanteId = sesion.Participantes.Single().ParticipanteId;

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var act = () => _sut.Handle(
            new ExpulsarParticipanteCommand(
                sesion.SesionId.Valor,
                participanteId.Valor,
                "Tarde"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*EnPreparacion*");
    }
}
