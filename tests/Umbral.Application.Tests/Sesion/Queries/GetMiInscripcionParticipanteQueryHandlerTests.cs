using FluentAssertions;
using NSubstitute;
using Umbral.Application.Sesion.Queries.GetMiInscripcionParticipante;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Queries;

public sealed class GetMiInscripcionParticipanteQueryHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly GetMiInscripcionParticipanteQueryHandler _sut;

    public GetMiInscripcionParticipanteQueryHandlerTests() =>
        _sut = new GetMiInscripcionParticipanteQueryHandler(_sesionRepo);

    [Fact]
    public async Task Handle_SinInscripcion_RetornaNull()
    {
        _sesionRepo
            .FindInscripcionVigentePorJugadorAsync(Arg.Any<UsuarioId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var result = await _sut.Handle(
            new GetMiInscripcionParticipanteQuery(Guid.NewGuid()),
            CancellationToken.None);

        result.Should().BeNull();
        await _sesionRepo.Received(1).EliminarParticipacionesEnSesionesTerminalesAsync(
            Arg.Any<UsuarioId>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConInscripcion_RetornaDto()
    {
        var jugadorId = Guid.NewGuid();
        var sesion = SesionTestBuilder.ConParticipante("Alpha", jugadorId);
        var participanteId = sesion.Participantes.Single().ParticipanteId;

        _sesionRepo
            .FindInscripcionVigentePorJugadorAsync(Arg.Any<UsuarioId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var result = await _sut.Handle(
            new GetMiInscripcionParticipanteQuery(jugadorId),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.SesionId.Should().Be(sesion.SesionId.Valor);
        result.ParticipanteId.Should().Be(participanteId.Valor);
        result.Estado.Should().Be("EnPreparacion");
        result.ParticipantesInscritos.Should().Be(1);
        result.MaxParticipantes.Should().Be(SesionAR.MaxParticipantes);
    }

    [Fact]
    public async Task Handle_ConPenalizacion_MapeaHistorial()
    {
        var jugadorId = Guid.NewGuid();
        var sesion = SesionTestBuilder.ConParticipante("Alpha", jugadorId);
        sesion.Iniciar();
        var participanteId = sesion.Participantes.Single().ParticipanteId;
        sesion.AplicarPenalizacion(
            participanteId,
            new Penalizacion(10, "Retraso", UsuarioId.Nuevo()));

        _sesionRepo
            .FindInscripcionVigentePorJugadorAsync(Arg.Any<UsuarioId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var result = await _sut.Handle(
            new GetMiInscripcionParticipanteQuery(jugadorId),
            CancellationToken.None);

        result!.Penalizaciones.Should().ContainSingle(p =>
            p.Puntos == 10 && p.Motivo == "Retraso");
    }

    [Fact]
    public async Task Handle_SesionFinalizada_RetornaDtoParaResultados()
    {
        var jugadorId = Guid.NewGuid();
        var sesion = SesionTestBuilder.ConParticipante("Alpha", jugadorId);
        sesion.Iniciar();
        sesion.Finalizar();

        _sesionRepo
            .FindInscripcionVigentePorJugadorAsync(Arg.Any<UsuarioId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var result = await _sut.Handle(
            new GetMiInscripcionParticipanteQuery(jugadorId),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Estado.Should().Be("Finalizada");
        await _sesionRepo.DidNotReceive().EliminarParticipacionesEnSesionesTerminalesAsync(
            Arg.Any<UsuarioId>(),
            Arg.Any<CancellationToken>());
    }
}
