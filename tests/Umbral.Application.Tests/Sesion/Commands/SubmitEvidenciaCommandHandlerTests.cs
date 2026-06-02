using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.SubmitEvidencia;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-18 — SubmitEvidencia (Application).</summary>
public sealed class SubmitEvidenciaCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly SubmitEvidenciaCommandHandler _sut;

    public SubmitEvidenciaCommandHandlerTests()
    {
        _sut = new SubmitEvidenciaCommandHandler(_sesionRepo, _publisher);
    }

    [Fact]
    public async Task Handle_CuandoSesionActivaYQrValido_RegistraEvidenciaYPublicaEventos()
    {
        // Arrange
        var sesion = SesionTestBuilder.Activa("Alpha");
        var participante = sesion.Participantes.First();
        var qrValido = SesionTestBuilder.CodigoQrEtapaActual(sesion);
        sesion.ClearDomainEvents();

        _sesionRepo
            .FindByIdAsync(Arg.Is<SesionId>(id => id.Valor == sesion.SesionId.Valor), Arg.Any<CancellationToken>())
            .Returns(sesion);
        _sesionRepo
            .SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        IReadOnlyList<IDomainEvent>? eventos = null;
        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                eventos = ci.ArgAt<IReadOnlyList<IDomainEvent>>(0).ToList();
                return Task.CompletedTask;
            });

        // Act
        var result = await _sut.Handle(
            new SubmitEvidenciaCommand(
                sesion.SesionId.Valor,
                participante.ParticipanteId.Valor,
                qrValido),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EvidenciaId.Should().NotBeEmpty();
        result.Value.Resultado.Should().Be(ResultadoValidacion.Valida);
        eventos.Should().Contain(e => e is EvidenciaRegistrada);
        sesion.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        // Act
        var act = () => _sut.Handle(
            new SubmitEvidenciaCommand(Guid.NewGuid(), Guid.NewGuid(), "QR-TEST"),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoParticipanteNoPerteneceALaSesion_LanzaDomainException()
    {
        // Arrange
        var sesion = SesionTestBuilder.Activa("Alpha");
        var qrValido = SesionTestBuilder.CodigoQrEtapaActual(sesion);

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        // Act
        var act = () => _sut.Handle(
            new SubmitEvidenciaCommand(
                sesion.SesionId.Valor,
                Guid.NewGuid(),
                qrValido),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_CuandoPrimeraEvidenciaValida_EmiteEventosGanadorYTransicion()
    {
        // Arrange
        var sesion = SesionTestBuilder.ActivaConParticipantesDosEtapas("Alpha", "Beta");
        var participanteGanador = sesion.Participantes.First();
        var qrValido = SesionTestBuilder.CodigoQrEtapaActual(sesion);

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
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

        // Act
        var result = await _sut.Handle(
            new SubmitEvidenciaCommand(
                sesion.SesionId.Valor,
                participanteGanador.ParticipanteId.Valor,
                qrValido),
            CancellationToken.None);

        // Assert
        result.Value.Resultado.Should().Be(ResultadoValidacion.Valida);
        sesion.ContextoMision!.EtapaActualIndex.Should().Be(1);
        eventos.Should().Contain(e => e is EvidenciaRegistrada);
        eventos.Should().Contain(e => e is EvidenciaValidada);
        eventos.Should().Contain(e => e is EtapaCompletada);
    }

    [Fact]
    public async Task Handle_CuandoSegundaEvidenciaUsaQrEtapaAnterior_ResultadoInvalida()
    {
        // Arrange
        var sesion = SesionTestBuilder.ActivaConParticipantesDosEtapas("Alpha", "Beta");
        var equipoUno = sesion.Participantes[0];
        var equipoDos = sesion.Participantes[1];
        var qrEtapaUno = SesionTestBuilder.CodigoQrEtapaActual(sesion);

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);
        _sesionRepo
            .SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Primera evidencia válida: define ganador y avanza etapa.
        await _sut.Handle(
            new SubmitEvidenciaCommand(
                sesion.SesionId.Valor,
                equipoUno.ParticipanteId.Valor,
                qrEtapaUno),
            CancellationToken.None);

        IReadOnlyList<IDomainEvent>? eventosSegunda = null;
        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                eventosSegunda = ci.ArgAt<IReadOnlyList<IDomainEvent>>(0).ToList();
                return Task.CompletedTask;
            });

        // Act
        var segunda = await _sut.Handle(
            new SubmitEvidenciaCommand(
                sesion.SesionId.Valor,
                equipoDos.ParticipanteId.Valor,
                qrEtapaUno),
            CancellationToken.None);

        // Assert
        segunda.Value.Resultado.Should().Be(ResultadoValidacion.Invalida);
        eventosSegunda.Should().ContainSingle(e => e is EvidenciaRegistrada);
    }
}
