using FluentAssertions;
using FluentValidation.TestHelper;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.SubmitRespuestaTrivia;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-34 — SubmitRespuestaTrivia validator.</summary>
public sealed class SubmitRespuestaTriviaValidatorTests
{
    private readonly SubmitRespuestaTriviaValidator _sut = new();

    [Fact]
    public void Validate_CuandoIndiceNegativo_Falla()
    {
        var result = _sut.TestValidate(
            new SubmitRespuestaTriviaCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -1));

        result.ShouldHaveValidationErrorFor(x => x.IndiceOpcion);
    }

    [Fact]
    public void Validate_CuandoDatosValidos_Pasa()
    {
        var result = _sut.TestValidate(
            new SubmitRespuestaTriviaCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0));

        result.ShouldNotHaveAnyValidationErrors();
    }
}

/// <summary>HU-34/35 — SubmitRespuestaTrivia encola mensaje.</summary>
public sealed class SubmitRespuestaTriviaCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IRespuestaTriviaBus _bus = Substitute.For<IRespuestaTriviaBus>();
    private readonly SubmitRespuestaTriviaCommandHandler _sut;

    public SubmitRespuestaTriviaCommandHandlerTests() =>
        _sut = new SubmitRespuestaTriviaCommandHandler(_sesionRepo, _bus);

    [Fact]
    public async Task Handle_CuandoInscrito_EncolaMensajeYRetornaAccepted()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var sesion = SesionTestBuilder.ActivaTrivia([pregunta.PreguntaId]);
        var participante = sesion.Participantes.First();
        sesion.IniciarSecuenciaTrivia(DateTimeOffset.UtcNow, 30);

        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var result = await _sut.Handle(
            new SubmitRespuestaTriviaCommand(
                sesion.SesionId.Valor,
                participante.JugadorId.Valor,
                pregunta.PreguntaId.Valor,
                0,
                30),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Accepted");
        result.Value.MessageId.Should().NotBeEmpty();
        await _bus.Received(1).PublicarAsync(
            Arg.Is<RespuestaTriviaMensaje>(m =>
                m.SesionId == sesion.SesionId.Valor
                && m.JugadorId == participante.JugadorId.Valor
                && m.PreguntaId == pregunta.PreguntaId.Valor
                && m.IndiceOpcion == 0),
            Arg.Any<CancellationToken>());
        await _sesionRepo.DidNotReceive().SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new SubmitRespuestaTriviaCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoJugadorNoInscrito_LanzaDomainException()
    {
        var pregunta = TriviaTestBuilder.PreguntaSinCategoria();
        var sesion = SesionTestBuilder.ActivaTrivia([pregunta.PreguntaId]);
        sesion.IniciarSecuenciaTrivia(DateTimeOffset.UtcNow, 30);
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var act = () => _sut.Handle(
            new SubmitRespuestaTriviaCommand(
                sesion.SesionId.Valor,
                Guid.NewGuid(),
                pregunta.PreguntaId.Valor,
                0),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*inscrito*");
    }
}
