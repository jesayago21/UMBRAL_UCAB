using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Queries.GetSesionOperador;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Queries;

public sealed class GetSesionOperadorQueryHandlerTests
{
    private readonly ISesionRepository _repo = Substitute.For<ISesionRepository>();
    private readonly GetSesionOperadorQueryHandler _sut;

    public GetSesionOperadorQueryHandlerTests()
        => _sut = new GetSesionOperadorQueryHandler(_repo);

    [Fact]
    public async Task Handle_SesionBt_RetornaDetalleConEtapas()
    {
        var sesion = SesionTestBuilder.Activa("Alpha");
        _repo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var dto = await _sut.Handle(new GetSesionOperadorQuery(sesion.SesionId.Valor), CancellationToken.None);

        dto.MisionNombre.Should().Be("Misión de prueba");
        dto.Etapas.Should().NotBeNull();
        dto.Etapas![0].Pistas.Should().NotBeNull();
        dto.Participantes.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_SesionConEtapaTrivia_RetornaDescripcionTrivia()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var mision = MisionTestBuilder.ActivaMixta(categoria.CategoriaId);
        var trivia = (EtapaTrivia)mision.Etapas[1];
        var snapshot = MisionSnapshot.Desde(
            mision,
            new Dictionary<EtapaId, EtapaTriviaSnapshot>
            {
                [trivia.EtapaId] = EtapaTriviaSnapshot.DesdeEtapa(
                    trivia,
                    [PreguntaId.Nuevo()],
                    categoria.Nombre)
            });

        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        sesion.AbrirParaRegistro();
        sesion.UnirseParticipante(UsuarioId.Nuevo(), "Alpha", sesion.CodigoAcceso.Valor);
        sesion.Iniciar();
        sesion.RegistrarEvidencia(sesion.Participantes[0].ParticipanteId, "QR-MIX-001");

        _repo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var dto = await _sut.Handle(new GetSesionOperadorQuery(sesion.SesionId.Valor), CancellationToken.None);

        dto.EtapaActivaTipo.Should().Be("Trivia");
        dto.EtapaActualDescripcion.Should().Contain("Trivia");
        dto.Etapas.Should().HaveCount(2);
        dto.Etapas![1].CategoriaIds.Should().Contain(categoria.CategoriaId.Valor);
    }

    [Fact]
    public async Task Handle_SesionLegacyBt_MapeaContextoLegacy()
    {
        var snapshot = MisionSnapshot.DesdeSoloBusquedaTesoro(MisionTestBuilder.Activa());
        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        typeof(SesionAR).GetProperty(nameof(SesionAR.ContextoMision))!.SetValue(sesion, null);
        typeof(SesionAR).GetProperty(nameof(SesionAR.ContextoBT))!.SetValue(
            sesion,
            ContextoBusquedaTesoro.Crear(snapshot));

        _repo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var dto = await _sut.Handle(new GetSesionOperadorQuery(sesion.SesionId.Valor), CancellationToken.None);

        dto.MisionNombre.Should().Be("Misión de prueba");
        dto.EtapaActivaTipo.Should().Be("BusquedaTesoro");
    }

    [Fact]
    public async Task Handle_SesionLegacyTrivia_MapeaContextoTrivia()
    {
        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(MisionTestBuilder.Activa()),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        typeof(SesionAR).GetProperty(nameof(SesionAR.ContextoMision))!.SetValue(sesion, null);
        typeof(SesionAR).GetProperty(nameof(SesionAR.ContextoTrivia))!.SetValue(
            sesion,
            ContextoTrivia.Crear([PreguntaId.Nuevo(), PreguntaId.Nuevo()], "Historia"));

        _repo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>()).Returns(sesion);

        var dto = await _sut.Handle(new GetSesionOperadorQuery(sesion.SesionId.Valor), CancellationToken.None);

        dto.MisionNombre.Should().Be("Historia");
        dto.EtapaActivaTipo.Should().Be("Trivia");
        dto.EtapaActualDescripcion.Should().Contain("Pregunta");
    }

    [Fact]
    public async Task Handle_CuandoNoExiste_LanzaNotFoundException()
    {
        _repo.FindByIdAsync(Arg.Any<SesionId>(), Arg.Any<CancellationToken>())
            .Returns((SesionAR?)null);

        var act = () => _sut.Handle(new GetSesionOperadorQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
