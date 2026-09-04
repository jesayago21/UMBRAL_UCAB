using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.LanzarPreguntaTrivia;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-33 — LanzarPreguntaTrivia.</summary>
public sealed class LanzarPreguntaTriviaCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly LanzarPreguntaTriviaCommandHandler _sut;

    public LanzarPreguntaTriviaCommandHandlerTests() =>
        _sut = new LanzarPreguntaTriviaCommandHandler(_sesionRepo, _preguntaRepo, _notifier);

    [Fact]
    public async Task Handle_CuandoSesionTriviaActiva_LanzaYNotifica()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria("¿Capital?");
        var sesion = SesionTestBuilder.ActivaTrivia([pregunta.PreguntaId]);
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);
        _preguntaRepo.FindByIdAsync(pregunta.PreguntaId, Arg.Any<CancellationToken>()).Returns(pregunta);

        var result = await _sut.Handle(
            new LanzarPreguntaTriviaCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesion.ContextoMision!.ObtenerFaseTrivia().Should().Be(FaseTrivia.PreguntaActiva);
        await _sesionRepo.Received(1).SaveAsync(sesion, Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarPreguntaTriviaIniciadaAsync(
            sesion.SesionId.Valor.ToString(),
            pregunta.PreguntaId.Valor.ToString(),
            1,
            pregunta.Enunciado,
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<DateTime>(),
            1,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new LanzarPreguntaTriviaCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoEtapaBt_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.Activa();
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var act = () => _sut.Handle(
            new LanzarPreguntaTriviaCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Trivia*");
    }
}
