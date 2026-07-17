using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.CerrarPreguntaTrivia;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-33 — CerrarPreguntaTrivia.</summary>
public sealed class CerrarPreguntaTriviaCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly CerrarPreguntaTriviaCommandHandler _sut;

    public CerrarPreguntaTriviaCommandHandlerTests() =>
        _sut = new CerrarPreguntaTriviaCommandHandler(_sesionRepo, _notifier);

    [Fact]
    public async Task Handle_TrasLanzar_EntraEnTransicionYNotifica()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var sesion = SesionTestBuilder.ActivaTrivia([pregunta.PreguntaId]);
        sesion.LanzarPreguntaTrivia(DateTimeOffset.UtcNow, 30);
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var result = await _sut.Handle(
            new CerrarPreguntaTriviaCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesion.ContextoMision!.ObtenerFaseTrivia().Should().Be(FaseTrivia.Transicion);
        await _notifier.Received(1).NotificarTriviaEnTransicionAsync(
            sesion.SesionId.Valor.ToString(),
            1,
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
            new CerrarPreguntaTriviaCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_SinPreguntaActiva_LanzaDomainException()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var sesion = SesionTestBuilder.ActivaTrivia([pregunta.PreguntaId]);
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var act = () => _sut.Handle(
            new CerrarPreguntaTriviaCommand(sesion.SesionId.Valor),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
