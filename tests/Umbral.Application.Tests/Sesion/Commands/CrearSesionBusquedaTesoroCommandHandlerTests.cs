using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.CrearSesionBusquedaTesoro;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

/// <summary>
/// HU-12 — CrearSesionBusquedaTesoro (capa Application).
/// RB-01: misión activa. Orden: persistir → PublishBatchAsync → ClearDomainEvents.
/// </summary>
public sealed class CrearSesionBusquedaTesoroCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IMisionRepository _misionRepo = Substitute.For<IMisionRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly CrearSesionBusquedaTesoroCommandHandler _sut;

    public CrearSesionBusquedaTesoroCommandHandlerTests()
    {
        _sut = new CrearSesionBusquedaTesoroCommandHandler(
            _sesionRepo,
            _misionRepo,
            _publisher);
    }

    [Fact]
    public async Task Handle_CuandoMisionActiva_CreaSesionYPublicaEvento()
    {
        var mision = MisionTestBuilder.Activa();
        var operadorId = Guid.NewGuid();

        _misionRepo
            .FindByIdAsync(
                Arg.Is<MisionId>(id => id.Valor == mision.MisionId.Valor),
                Arg.Any<CancellationToken>())
            .Returns(mision);

        SesionAR? sesionGuardada = null;
        _sesionRepo
            .SaveAsync(Arg.Do<SesionAR>(s => sesionGuardada = s), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        IReadOnlyList<IDomainEvent>? eventosPublicados = null;
        _publisher
            .PublishBatchAsync(Arg.Any<IReadOnlyList<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                eventosPublicados = callInfo
                    .ArgAt<IReadOnlyList<IDomainEvent>>(0)
                    .ToList();
                return Task.CompletedTask;
            });

        var command = new CrearSesionBusquedaTesoroCommand(
            mision.MisionId.Valor,
            operadorId);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.CodigoAcceso.Should().NotBeNullOrWhiteSpace();

        await _sesionRepo.Received(1).SaveAsync(
            Arg.Any<SesionAR>(),
            Arg.Any<CancellationToken>());

        await _publisher.Received(1).PublishBatchAsync(
            Arg.Any<IReadOnlyList<IDomainEvent>>(),
            Arg.Any<CancellationToken>());

        eventosPublicados.Should().NotBeNull();
        eventosPublicados!.Should().ContainSingle()
            .Which.Should().BeOfType<SesionCreada>();

        sesionGuardada.Should().NotBeNull();
        sesionGuardada!.DomainEvents.Should().BeEmpty();
        sesionGuardada.OperadorId.Valor.Should().Be(operadorId);
        sesionGuardada.Estado.Should().Be(EstadoSesion.Programada);
        sesionGuardada.CodigoAcceso.Valor.Should().Be(result.Value.CodigoAcceso);
    }

    [Fact]
    public async Task Handle_CuandoMisionNoExiste_LanzaNotFoundException()
    {
        _misionRepo
            .FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns((Mision?)null);

        var command = new CrearSesionBusquedaTesoroCommand(
            Guid.NewGuid(),
            Guid.NewGuid());

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        await _sesionRepo.DidNotReceive().SaveAsync(
            Arg.Any<SesionAR>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoMisionInactiva_LanzaDomainException()
    {
        var mision = MisionTestBuilder.Inactiva();

        _misionRepo
            .FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns(mision);

        var command = new CrearSesionBusquedaTesoroCommand(
            mision.MisionId.Valor,
            Guid.NewGuid());

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();

        await _sesionRepo.DidNotReceive().SaveAsync(
            Arg.Any<SesionAR>(),
            Arg.Any<CancellationToken>());
    }
}
