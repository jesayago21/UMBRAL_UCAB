using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Sesion.Commands.CrearSesionTrivia;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Ports;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Application.Tests.Sesion.Commands;

public sealed class CrearSesionTriviaCommandHandlerTests
{
    private readonly ISesionRepository _sesionRepo = Substitute.For<ISesionRepository>();
    private readonly ICategoriaRepository _categoriaRepo = Substitute.For<ICategoriaRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    private readonly CrearSesionTriviaCommandHandler _sut;

    public CrearSesionTriviaCommandHandlerTests()
    {
        _sut = new CrearSesionTriviaCommandHandler(
            _sesionRepo,
            _categoriaRepo,
            _preguntaRepo,
            _publisher);
    }

    [Fact]
    public async Task Handle_ConCategoriaConPreguntas_CreaSesionTrivia()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var pregunta  = TriviaTestBuilder.PreguntaConCategoria(categoria.CategoriaId);
        var operadorId = Guid.NewGuid();

        _categoriaRepo
            .FindByIdAsync(
                Arg.Is<CategoriaId>(id => id.Valor == categoria.CategoriaId.Valor),
                Arg.Any<CancellationToken>())
            .Returns(categoria);

        _preguntaRepo
            .FindByCategoriaAsync(categoria.CategoriaId, Arg.Any<CancellationToken>())
            .Returns([pregunta]);

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

        var command = new CrearSesionTriviaCommand(
            [categoria.CategoriaId.Valor],
            operadorId);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        sesionGuardada.Should().NotBeNull();
        sesionGuardada!.TipoSesion.Should().Be(TipoSesion.Trivia);
        sesionGuardada.ContextoTrivia!.TotalPreguntas.Should().Be(1);

        eventosPublicados.Should().NotBeNull();
        eventosPublicados!.Should().ContainSingle().Which.Should().BeOfType<SesionCreada>();
    }

    [Fact]
    public async Task Handle_CuandoCategoriaNoExiste_LanzaNotFoundException()
    {
        var id = Guid.NewGuid();
        _categoriaRepo
            .FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns((Categoria?)null);

        var command = new CrearSesionTriviaCommand([id], Guid.NewGuid());

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CuandoCategoriaSinPreguntasActivas_LanzaDomainException()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();

        _categoriaRepo
            .FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns(categoria);

        _preguntaRepo
            .FindByCategoriaAsync(categoria.CategoriaId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Pregunta>());

        var command = new CrearSesionTriviaCommand(
            [categoria.CategoriaId.Valor],
            Guid.NewGuid());

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
