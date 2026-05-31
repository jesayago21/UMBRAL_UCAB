using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.UnirseSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class UnirseSesionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly UnirseSesionCommandHandler _sut;

    public UnirseSesionCommandHandlerTests()
    {
        _sut = new UnirseSesionCommandHandler(_sesionRepo, _publisher);
    }

    [Fact]
    public async Task Handle_CuandoSesionExisteYCodigoValido_UneEquipoYPersiste()
    {
        var sesion = SesionTestBuilder.EnPreparacionSinEquipos();
        var jugadorId = Guid.NewGuid();

        _sesionRepo
            .FindByIdAsync(
                Arg.Is<SesionId>(id => id.Valor == sesion.SesionId.Valor),
                Arg.Any<CancellationToken>())
            .Returns(sesion);

        _sesionRepo
            .SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new UnirseSesionCommand(
            sesion.SesionId.Valor,
            sesion.CodigoAcceso.Valor,
            jugadorId,
            "Alpha");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EquipoId.Should().NotBeEmpty();
        sesion.Equipos.Should().ContainSingle(e =>
            e.Nombre.Valor == "Alpha" && e.JugadorId.Valor == jugadorId);

        await _sesionRepo.Received(1).SaveAsync(sesion, Arg.Any<CancellationToken>());
        sesion.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CuandoSesionProgramada_AbreRegistroYUneEquipo()
    {
        var sesion = SesionAR.CrearBusquedaTesoro(
            MisionSnapshot.Desde(MisionTestBuilder.Activa()),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        sesion.Estado.Should().Be(EstadoSesion.Programada);

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        _sesionRepo
            .SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new UnirseSesionCommand(
                sesion.SesionId.Valor,
                sesion.CodigoAcceso.Valor,
                Guid.NewGuid(),
                "Beta"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesion.Estado.Should().Be(EstadoSesion.EnPreparacion);
        sesion.Equipos.Should().ContainSingle(e => e.Nombre.Valor == "Beta");
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var command = new UnirseSesionCommand(
            Guid.NewGuid(),
            "ABC123",
            Guid.NewGuid(),
            "Alpha");

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        await _sesionRepo.DidNotReceive().SaveAsync(
            Arg.Any<SesionAR>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoNombreDuplicado_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.ConEquipo("Alpha");

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var command = new UnirseSesionCommand(
            sesion.SesionId.Valor,
            sesion.CodigoAcceso.Valor,
            Guid.NewGuid(),
            "alpha");

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();

        await _sesionRepo.DidNotReceive().SaveAsync(
            Arg.Any<SesionAR>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoSesionFinalizada_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.Finalizada();

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var command = new UnirseSesionCommand(
            sesion.SesionId.Valor,
            sesion.CodigoAcceso.Valor,
            Guid.NewGuid(),
            "Beta");

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*sesión cerrada*");

        await _sesionRepo.DidNotReceive().SaveAsync(
            Arg.Any<SesionAR>(),
            Arg.Any<CancellationToken>());
    }
}
