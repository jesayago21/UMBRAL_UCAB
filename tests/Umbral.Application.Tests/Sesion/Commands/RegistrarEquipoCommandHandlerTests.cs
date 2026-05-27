using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.RegistrarEquipo;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>
/// HU-13 — RegistrarEquipo (Application). RB-02, RB-03.
/// </summary>
public sealed class RegistrarEquipoCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly RegistrarEquipoCommandHandler _sut;

    public RegistrarEquipoCommandHandlerTests()
    {
        _sut = new RegistrarEquipoCommandHandler(_sesionRepo, _publisher);
    }

    [Fact]
    public async Task Handle_CuandoSesionExisteYNombreUnico_RegistraEquipoYPersiste()
    {
        var sesion = SesionTestBuilder.EnPreparacionSinEquipos();

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

        var command = new RegistrarEquipoCommand(
            sesion.SesionId.Valor,
            "Los Sabuesos");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.EquipoId.Should().NotBeEmpty();
        result.Value.CodigoAcceso.Should().NotBeNullOrWhiteSpace();
        sesion.Equipos.Should().ContainSingle(e => e.Nombre.Valor == "Los Sabuesos");

        await _sesionRepo.Received(1).SaveAsync(sesion, Arg.Any<CancellationToken>());
        sesion.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CuandoSesionProgramada_AbreRegistroYRegistraEquipo()
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
            new RegistrarEquipoCommand(sesion.SesionId.Valor, "Exploradores"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesion.Estado.Should().Be(EstadoSesion.EnPreparacion);
        sesion.Equipos.Should().ContainSingle(e => e.Nombre.Valor == "Exploradores");
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var command = new RegistrarEquipoCommand(Guid.NewGuid(), "Alpha");

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        await _sesionRepo.DidNotReceive().SaveAsync(
            Arg.Any<SesionAR>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoNombreDuplicado_LanzaDomainException()
    {
        var sesion = SesionTestBuilder.ConEquipo("Los Sabuesos");

        _sesionRepo
            .FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns(sesion);

        var command = new RegistrarEquipoCommand(
            sesion.SesionId.Valor,
            "los sabuesos");

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

        var command = new RegistrarEquipoCommand(
            sesion.SesionId.Valor,
            "Nuevo Equipo");

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*sesión cerrada*");

        await _sesionRepo.DidNotReceive().SaveAsync(
            Arg.Any<SesionAR>(),
            Arg.Any<CancellationToken>());
    }
}
