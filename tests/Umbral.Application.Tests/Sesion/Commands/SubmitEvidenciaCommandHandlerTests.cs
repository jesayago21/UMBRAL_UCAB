using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.SubmitEvidencia;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;
// ResultadoValidacion no se importa aquí; el resultado se compara como string (V1 auditoría).

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>HU-18 — SubmitEvidencia (Application).</summary>
public sealed class SubmitEvidenciaCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly INotificacionRealTime _notifier = Substitute.For<INotificacionRealTime>();
    private readonly SubmitEvidenciaCommandHandler _sut;

    public SubmitEvidenciaCommandHandlerTests()
    {
        _sut = new SubmitEvidenciaCommandHandler(_sesionRepo, _publisher, _notifier);
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
                participante.JugadorId.Valor,
                qrValido),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EvidenciaId.Should().NotBeEmpty();
        result.Value.Resultado.Should().Be("Valida");
        eventos.Should().Contain(e => e is EvidenciaRegistrada);
        sesion.DomainEvents.Should().BeEmpty();
        await _notifier.Received(1).NotificarRankingActualizadoAsync(
            sesion.SesionId.Valor.ToString(),
            Arg.Any<IReadOnlyList<RankingPosicionNotificacion>>(),
            Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarEtapaAvanzadaAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
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
    public async Task Handle_CuandoJugadorNoPerteneceALaSesion_LanzaDomainException()
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
                participanteGanador.JugadorId.Valor,
                qrValido),
            CancellationToken.None);

        // Assert
        result.Value.Resultado.Should().Be("Valida");
        sesion.ContextoMision!.EtapaActualIndex.Should().Be(1);
        eventos.Should().Contain(e => e is EvidenciaRegistrada);
        eventos.Should().Contain(e => e is EvidenciaValidada);
        eventos.Should().Contain(e => e is EtapaCompletada);
        await _notifier.Received(1).NotificarRankingActualizadoAsync(
            sesion.SesionId.Valor.ToString(),
            Arg.Any<IReadOnlyList<RankingPosicionNotificacion>>(),
            Arg.Any<CancellationToken>());
        await _notifier.Received(1).NotificarEtapaAvanzadaAsync(
            sesion.SesionId.Valor.ToString(),
            1,
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
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
                equipoUno.JugadorId.Valor,
                qrEtapaUno),
            CancellationToken.None);

        _notifier.ClearReceivedCalls();

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
                equipoDos.JugadorId.Valor,
                qrEtapaUno),
            CancellationToken.None);

        // Assert
        segunda.Value.Resultado.Should().Be("Invalida");
        eventosSegunda.Should().ContainSingle(e => e is EvidenciaRegistrada);
        await _notifier.DidNotReceive().NotificarRankingActualizadoAsync(
            Arg.Any<string>(),
            Arg.Any<IReadOnlyList<RankingPosicionNotificacion>>(),
            Arg.Any<CancellationToken>());
        await _notifier.DidNotReceive().NotificarEtapaAvanzadaAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoGanaEtapaConPistaAlInicioEnSiguiente_NotificaPistaLiberadaATodos()
    {
        var sesion = CrearSesionActivaDosEtapasConPistaAlInicio();
        var ganador = sesion.Participantes[0];
        var otro = sesion.Participantes[1];
        var qrValido = SesionTestBuilder.CodigoQrEtapaActual(sesion);

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);
        _sesionRepo
            .SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _notifier
            .NotificarPistaLiberadaAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new SubmitEvidenciaCommand(
                sesion.SesionId.Valor,
                ganador.JugadorId.Valor,
                qrValido),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Resultado.Should().Be("Valida");
        sesion.ContextoMision!.EtapaActualIndex.Should().Be(1);
        sesion.ContextoMision.PistasEntregadas.Where(p => p.EtapaIndex == 1).Should().HaveCount(2);
        await _notifier.Received().NotificarPistaLiberadaAsync(
            sesion.SesionId.Valor.ToString(),
            ganador.ParticipanteId.Valor.ToString(),
            Arg.Any<string>(),
            1,
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        await _notifier.Received().NotificarPistaLiberadaAsync(
            sesion.SesionId.Valor.ToString(),
            otro.ParticipanteId.Valor.ToString(),
            Arg.Any<string>(),
            1,
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    private static SesionAR CrearSesionActivaDosEtapasConPistaAlInicio()
    {
        var mision = Mision.Crear("AlInicio handler");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-H10-1");
        mision.AgregarEtapaBusquedaTesoro("Etapa 2", "QR-H10-2");
        ((EtapaBusquedaTesoro)mision.Etapas[1])
            .AgregarPista("Pista inicial etapa 2", TipoLiberacion.PorGanador);
        mision.Activar();
        mision.ClearDomainEvents();

        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(mision),
            UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        sesion.UnirseParticipante(UsuarioId.Nuevo(), "Alpha", sesion.CodigoAcceso.Valor);
        sesion.UnirseParticipante(UsuarioId.Nuevo(), "Beta", sesion.CodigoAcceso.Valor);
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }
}
