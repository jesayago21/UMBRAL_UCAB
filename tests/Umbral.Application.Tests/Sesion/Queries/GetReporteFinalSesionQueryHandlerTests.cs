using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Queries.GetReporteFinalSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Sesion;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Queries;

public sealed class GetReporteFinalSesionQueryHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly GetReporteFinalSesionQueryHandler _sut;

    public GetReporteFinalSesionQueryHandlerTests()
    {
        _sut = new GetReporteFinalSesionQueryHandler(_sesionRepo);
    }

    [Fact]
    public async Task Handle_CuandoSesionFinalizada_RetornaRankingFinal()
    {
        var sesion = SesionTestBuilder.Finalizada();
        sesion.Participantes.First().SumarPuntaje(75);

        _sesionRepo.FindByIdAsync(sesion.SesionId, Arg.Any<CancellationToken>()).Returns(sesion);

        var result = await _sut.Handle(
            new GetReporteFinalSesionQuery(sesion.SesionId.Valor),
            CancellationToken.None);

        result.SesionId.Should().Be(sesion.SesionId.Valor);
        result.Estado.Should().Be("Finalizada");
        result.FinalizadaEn.Should().NotBeNull();
        result.Ranking.Should().ContainSingle();
        result.Ranking[0].PuntajeTotal.Should().Be(75);
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new GetReporteFinalSesionQuery(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
