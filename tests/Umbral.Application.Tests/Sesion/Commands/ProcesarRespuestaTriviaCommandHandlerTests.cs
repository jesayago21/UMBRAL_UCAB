using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.ProcesarRespuestaTrivia;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-35 — ProcesarRespuestaTrivia (consumer).</summary>
public sealed class ProcesarRespuestaTriviaCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly ProcesarRespuestaTriviaCommandHandler _sut;

    public ProcesarRespuestaTriviaCommandHandlerTests()
    {
        _sut = new ProcesarRespuestaTriviaCommandHandler(
            _sesionRepo,
            _preguntaRepo,
            _publisher,
            _notifier);
    }

    [Fact]
    public async Task Handle_CuandoRespuestaCorrectaATiempo_SumaPuntosYNotificaRanking()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria("¿Capital?");
        var sesion = SesionTestBuilder.ActivaTrivia([pregunta.PreguntaId]);
        var participante = sesion.Participantes.First();
        sesion.IniciarSecuenciaTrivia(DateTimeOffset.UtcNow, 30);
        sesion.ClearDomainEvents();

        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);
        _preguntaRepo.FindByIdAsync(pregunta.PreguntaId, Arg.Any<CancellationToken>()).Returns(pregunta);
        _publisher.PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var recibidoEn = DateTimeOffset.UtcNow;
        var result = await _sut.Handle(
            new ProcesarRespuestaTriviaCommand(
                sesion.SesionId.Valor,
                participante.JugadorId.Valor,
                pregunta.PreguntaId.Valor,
                0,
                30,
                recibidoEn),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EsCorrecta.Should().BeTrue();
        result.Value.FueraDeTiempo.Should().BeFalse();
        result.Value.PuntosOtorgados.Should().BeGreaterThan(0);
        await _sesionRepo.Received(1).SaveAsync(sesion, Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarRankingActualizadoAsync(
            sesion.SesionId.Valor.ToString(),
            Arg.Any<IReadOnlyList<RankingPosicionNotificacion>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoSegundaRespuesta_LanzaDomainException()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var sesion = SesionTestBuilder.ActivaTrivia([pregunta.PreguntaId]);
        var participante = sesion.Participantes.First();
        var t0 = DateTimeOffset.UtcNow;
        sesion.IniciarSecuenciaTrivia(t0, 30);
        sesion.RegistrarRespuestaTrivia(participante.ParticipanteId, pregunta, 0, t0.AddSeconds(2), 30);
        sesion.ClearDomainEvents();

        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);
        _preguntaRepo.FindByIdAsync(pregunta.PreguntaId, Arg.Any<CancellationToken>()).Returns(pregunta);

        var act = () => _sut.Handle(
            new ProcesarRespuestaTriviaCommand(
                sesion.SesionId.Valor,
                participante.JugadorId.Valor,
                pregunta.PreguntaId.Valor,
                1,
                30,
                DateTimeOffset.UtcNow),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*RB-13*");
    }
}
