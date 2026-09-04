using FluentAssertions;
using NSubstitute;
using Umbral.Application.Sesion.Commands.ProcesarCicloTriviaAutomatico;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-33/38 — motor automático de trivia.</summary>
public sealed class ProcesarCicloTriviaAutomaticoCommandHandlerTests
{
    private const int Duracion = 30;

    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly IEventPublisher _eventPublisher = Substitute.For<IEventPublisher>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly ProcesarCicloTriviaAutomaticoCommandHandler _sut;

    public ProcesarCicloTriviaAutomaticoCommandHandlerTests() =>
        _sut = new ProcesarCicloTriviaAutomaticoCommandHandler(
            _sesionRepo,
            _preguntaRepo,
            _eventPublisher,
            _notifier);

    [Fact]
    public async Task Handle_TrasUltimaPreguntaUltimaEtapa_FinalizaYNotifica()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria("¿Capital?");
        var sesion = SesionTestBuilder.ActivaTrivia([pregunta.PreguntaId]);
        var t0 = DateTimeOffset.Parse("2026-07-14T18:00:00Z");
        sesion.IniciarSecuenciaTrivia(t0, Duracion);
        sesion.ProcesarCicloTriviaAutomatico(t0.AddSeconds(Duracion + 1), Duracion, 5);

        _sesionRepo.FindActivasAsync(Arg.Any<CancellationToken>()).Returns([sesion]);

        var result = await _sut.Handle(
            new ProcesarCicloTriviaAutomaticoCommand(Ahora: t0.AddSeconds(Duracion + 1 + 5)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        sesion.Estado.Should().Be(EstadoSesion.Finalizada);
        await _sesionRepo.Received(1).SaveAsync(sesion, Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishBatchAsync(
            Arg.Any<IReadOnlyList<IDomainEvent>>(),
            Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarCambioEstadoSesionAsync(
            sesion.SesionId.Valor.ToString(),
            "Finalizada",
            Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarRankingActualizadoAsync(
            sesion.SesionId.Valor.ToString(),
            Arg.Any<IReadOnlyList<RankingPosicionNotificacion>>(),
            Arg.Any<CancellationToken>());
        await _notifier.DidNotReceive().NotificarTriviaEnTransicionAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }
}
