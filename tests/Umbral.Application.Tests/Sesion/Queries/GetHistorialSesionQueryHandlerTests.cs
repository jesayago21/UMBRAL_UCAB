using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Queries.GetHistorialSesion;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.Sesion;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Queries;

public sealed class GetHistorialSesionQueryHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly GetHistorialSesionQueryHandler _sut;

    public GetHistorialSesionQueryHandlerTests()
    {
        _sut = new GetHistorialSesionQueryHandler(_sesionRepo);
    }

    [Fact]
    public async Task Handle_CuandoSesionExiste_RetornaEventosPaginados()
    {
        var sesion = SesionTestBuilder.Activa("Alpha");
        var sesionId = sesion.SesionId;
        var eventos = sesion.HistorialEventos.Take(2).ToList();

        _sesionRepo.FindByIdAsync(sesionId, Arg.Any<CancellationToken>()).Returns(sesion);
        _sesionRepo.CountEventosHistorialAsync(sesionId, Arg.Any<CancellationToken>())
            .Returns(sesion.HistorialEventos.Count);
        _sesionRepo.ListEventosHistorialAsync(sesionId, 1, 50, Arg.Any<CancellationToken>())
            .Returns(eventos);

        var result = await _sut.Handle(
            new GetHistorialSesionQuery(sesionId.Valor, 1, 50),
            CancellationToken.None);

        result.SesionId.Should().Be(sesionId.Valor);
        result.TotalEventos.Should().BeGreaterThan(0);
        result.Eventos.Should().HaveCount(eventos.Count);
        result.Eventos[0].Tipo.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_CuandoSesionNoExiste_LanzaNotFoundException()
    {
        _sesionRepo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(
            new GetHistorialSesionQuery(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
