using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.CrearSesionMision;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class CrearSesionMisionCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly IMisionRepository _misionRepo = Substitute.For<IMisionRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly ICategoriaRepository _categoriaRepo = Substitute.For<ICategoriaRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly CrearSesionMisionCommandHandler _sut;

    public CrearSesionMisionCommandHandlerTests()
    {
        _sut = new CrearSesionMisionCommandHandler(
            _sesionRepo,
            _misionRepo,
            _preguntaRepo,
            _categoriaRepo,
            _publisher);
    }

    [Fact]
    public async Task Handle_CuandoMisionActiva_CreaSesionYPublicaEvento()
    {
        var mision = MisionTestBuilder.Activa();
        var operadorId = Guid.NewGuid();

        _misionRepo
            .FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
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

        var result = await _sut.Handle(
            new CrearSesionMisionCommand(mision.MisionId.Valor, operadorId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SesionId.Should().NotBeEmpty();
        result.Value.CodigoAcceso.Should().NotBeNullOrWhiteSpace();
        result.Value.MisionNombre.Should().Be(mision.Nombre);

        sesionGuardada.Should().NotBeNull();
        sesionGuardada!.DomainEvents.Should().BeEmpty();
        eventosPublicados!.Should().ContainSingle().Which.Should().BeOfType<SesionCreada>();
    }

    [Fact]
    public async Task Handle_CuandoMisionNoExiste_LanzaNotFoundException()
    {
        _misionRepo
            .FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns((Mision?)null);

        var act = () => _sut.Handle(
            new CrearSesionMisionCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _sesionRepo.DidNotReceive().SaveAsync(Arg.Any<SesionAR>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CuandoMisionInactiva_LanzaDomainException()
    {
        _misionRepo
            .FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns(MisionTestBuilder.Inactiva());

        var act = () => _sut.Handle(
            new CrearSesionMisionCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_MisionConEtapaTrivia_ResuelvePreguntasEnSnapshot()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var pregunta = TriviaTestBuilder.PreguntaConCategoria(categoria.CategoriaId);
        var mision = MisionTestBuilder.ActivaConEtapaTrivia(categoria.CategoriaId);

        _misionRepo
            .FindByIdAsync(Arg.Any<MisionId>(), Arg.Any<CancellationToken>())
            .Returns(mision);

        _categoriaRepo
            .FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);

        _preguntaRepo
            .FindByCategoriaAsync(categoria.CategoriaId, Arg.Any<CancellationToken>())
            .Returns([pregunta]);

        SesionAR? sesionGuardada = null;
        _sesionRepo
            .SaveAsync(Arg.Do<SesionAR>(s => sesionGuardada = s), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new CrearSesionMisionCommand(mision.MisionId.Valor, Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var trivia = (EtapaTriviaSnapshot)sesionGuardada!.ContextoMision!.ObtenerEtapaActual();
        trivia.PreguntasOrdenadas.Should().ContainSingle();
    }
}
