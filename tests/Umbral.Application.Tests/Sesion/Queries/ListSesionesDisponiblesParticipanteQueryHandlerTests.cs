using FluentAssertions;
using NSubstitute;
using Umbral.Application.Sesion.Queries.ListSesionesDisponiblesParticipante;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Queries;

public sealed class ListSesionesDisponiblesParticipanteQueryHandlerTests
{
    private readonly ISesionRepository _repo = Substitute.For<ISesionRepository>();
    private readonly ListSesionesDisponiblesParticipanteQueryHandler _sut;

    public ListSesionesDisponiblesParticipanteQueryHandlerTests()
        => _sut = new ListSesionesDisponiblesParticipanteQueryHandler(_repo);

    [Fact]
    public async Task Handle_SesionMision_RetornaTituloDesdeSnapshot()
    {
        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(MisionTestBuilder.Activa()),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();

        _repo
            .FindDisponiblesParaParticipanteAsync(null, Arg.Any<CancellationToken>())
            .Returns([sesion]);

        var result = await _sut.Handle(
            new ListSesionesDisponiblesParticipanteQuery(null),
            CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Titulo.Should().Be("Misión de prueba");
        result[0].ParticipantesInscritos.Should().Be(0);
    }

    [Fact]
    public async Task Handle_SesionLegacyTrivia_RetornaTituloCategorias()
    {
        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.Rehydrate(
                MisionId.Nuevo(),
                "Legacy",
                [
                    EtapaTriviaSnapshot.Rehydrate(
                        EtapaId.Nuevo(),
                        1,
                        [],
                        [PreguntaId.Nuevo()],
                        "Historia")
                ]),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();

        _repo
            .FindDisponiblesParaParticipanteAsync(null, Arg.Any<CancellationToken>())
            .Returns([sesion]);

        var result = await _sut.Handle(
            new ListSesionesDisponiblesParticipanteQuery(null),
            CancellationToken.None);

        result[0].Titulo.Should().Be("Legacy");
    }

    [Fact]
    public async Task Handle_SesionLegacyBt_RetornaTituloDesdeContextoBt()
    {
        var snapshot = MisionSnapshot.DesdeSoloBusquedaTesoro(MisionTestBuilder.Activa());
        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        typeof(SesionAR).GetProperty(nameof(SesionAR.ContextoMision))!.SetValue(sesion, null);
        typeof(SesionAR).GetProperty(nameof(SesionAR.ContextoBT))!.SetValue(
            sesion,
            ContextoBusquedaTesoro.Crear(snapshot));

        _repo
            .FindDisponiblesParaParticipanteAsync(null, Arg.Any<CancellationToken>())
            .Returns([sesion]);

        var result = await _sut.Handle(
            new ListSesionesDisponiblesParticipanteQuery(null),
            CancellationToken.None);

        result[0].Titulo.Should().Be("Misión de prueba");
    }

    [Fact]
    public async Task Handle_SesionLegacyTriviaContexto_RetornaNombreSesion()
    {
        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(MisionTestBuilder.Activa()),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        typeof(SesionAR).GetProperty(nameof(SesionAR.ContextoMision))!.SetValue(sesion, null);
        typeof(SesionAR).GetProperty(nameof(SesionAR.ContextoTrivia))!.SetValue(
            sesion,
            ContextoTrivia.Crear([PreguntaId.Nuevo()], "Ciencia"));

        _repo
            .FindDisponiblesParaParticipanteAsync(null, Arg.Any<CancellationToken>())
            .Returns([sesion]);

        var result = await _sut.Handle(
            new ListSesionesDisponiblesParticipanteQuery(null),
            CancellationToken.None);

        result[0].Titulo.Should().Be("Misión de prueba");
    }
}
