using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Queries.GetRankingSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Sesion;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Queries;

public sealed class GetRankingSesionQueryHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly GetRankingSesionQueryHandler _sut;

    public GetRankingSesionQueryHandlerTests()
    {
        _sut = new GetRankingSesionQueryHandler(_sesionRepo);
    }

    [Fact]
    public async Task Handle_CuandoSesionExiste_RetornaRankingOrdenado()
    {
        // Arrange
        var sesion = SesionTestBuilder.ActivaConParticipantes("Alpha", "Beta");
        sesion.Participantes.First(e => e.Nombre.Valor == "Alpha").SumarPuntaje(50);
        sesion.Participantes.First(e => e.Nombre.Valor == "Beta").SumarPuntaje(90);

        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        // Act
        var result = await _sut.Handle(new GetRankingSesionQuery(sesion.SesionId.Valor), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].NombreParticipante.Should().Be("Beta");
        result[0].Posicion.Should().Be(1);
        result[1].NombreParticipante.Should().Be("Alpha");
        result[1].Posicion.Should().Be(2);
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        // Arrange
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns((SesionAR?)null);

        // Act
        var act = () => _sut.Handle(new GetRankingSesionQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
