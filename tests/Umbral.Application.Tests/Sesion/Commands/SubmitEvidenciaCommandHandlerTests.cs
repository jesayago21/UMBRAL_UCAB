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
        var equipo = sesion.Equipos.First();
        var qrValido = SesionTestBuilder.CodigoQrEtapaActual(sesion);
        sesion.ClearDomainEvents();

        _sesionRepo
            .FindByIdAsync(Arg.Is<SesionId>(id => id.Valor == sesion.SesionId.Valor), Arg.Any<CancellationToken>())
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
                equipo.EquipoId.Valor,
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
    public async Task Handle_CuandoEquipoNoPerteneceALaSesion_LanzaDomainException()
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
}
