using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.AbandonarSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class AbandonarSesionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly AbandonarSesionCommandHandler _sut;

    public AbandonarSesionCommandHandlerTests() => _sut = new AbandonarSesionCommandHandler(_sesionRepo);

    [Fact]
    public async Task Handle_CuandoInscrito_EliminaParticipanteYPersiste()
    {
        var jugadorId = Guid.NewGuid();
        var sesion = SesionTestBuilder.ConParticipante("Alpha", jugadorId);

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var result = await _sut.Handle(
            new AbandonarSesionCommand(sesion.SesionId.Valor, jugadorId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesion.Participantes.Should().BeEmpty();

        await _sesionRepo.Received(1).EliminarParticipanteAsync(
            Arg.Any<ParticipanteId>(),
            Arg.Any<CancellationToken>());
        await _sesionRepo.Received(1).SaveAsync(sesion, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new AbandonarSesionCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
