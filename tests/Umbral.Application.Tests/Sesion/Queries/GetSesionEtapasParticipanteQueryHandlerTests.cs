using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Queries.GetSesionEtapasParticipante;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Queries;

public sealed class GetSesionEtapasParticipanteQueryHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly GetSesionEtapasParticipanteQueryHandler _sut;

    public GetSesionEtapasParticipanteQueryHandlerTests() =>
        _sut = new GetSesionEtapasParticipanteQueryHandler(_sesionRepo);

    [Fact]
    public async Task Handle_ParticipanteInscrito_RetornaEtapas()
    {
        var jugadorId = Guid.NewGuid();
        var sesion = SesionTestBuilder.ConParticipante("Alpha", jugadorId);
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var dto = await _sut.Handle(
            new GetSesionEtapasParticipanteQuery(sesion.SesionId.Valor, jugadorId),
            CancellationToken.None);

        dto.Estado.Should().Be("EnPreparacion");
        dto.TotalEtapas.Should().BeGreaterThan(0);
        dto.Etapas.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_SinInscripcion_LanzaUnauthorized()
    {
        var sesion = SesionTestBuilder.ConParticipante("Alpha");
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var act = () => _sut.Handle(
            new GetSesionEtapasParticipanteQuery(sesion.SesionId.Valor, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_SesionInexistente_LanzaNotFound()
    {
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new GetSesionEtapasParticipanteQuery(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
