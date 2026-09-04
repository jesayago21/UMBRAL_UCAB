using FluentAssertions;
using NSubstitute;
using Umbral.Application.Common.Exceptions;
using Umbral.Application.Misiones.Commands.CrearMision;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Application.Tests.Misiones.Commands;

public sealed class CrearMisionCommandHandlerTests
{
    private readonly IMisionRepository _misionRepo = Substitute.For<IMisionRepository>();
    private readonly ICategoriaRepository _categoriaRepo = Substitute.For<ICategoriaRepository>();
    private readonly IPreguntaRepository _preguntaRepo = Substitute.For<IPreguntaRepository>();
    private readonly CrearMisionCommandHandler _sut;

    public CrearMisionCommandHandlerTests()
    {
        _sut = new CrearMisionCommandHandler(_misionRepo, _categoriaRepo, _preguntaRepo);
    }

    [Fact]
    public async Task Handle_ConEtapaTriviaYActivarSinPreguntas_LanzaDomainExceptionRb09()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        _categoriaRepo.FindByIdAsync(categoria.CategoriaId, Arg.Any<CancellationToken>()).Returns(categoria);
        _preguntaRepo
            .FindByCategoriaAsync(categoria.CategoriaId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Pregunta>());

        var act = () => _sut.Handle(
            new CrearMisionCommand(
                "Misión trivia vacía",
                [new EtapaMisionInput("Trivia", 1, null, null, null, [categoria.CategoriaId.Valor])],
                Activar: true),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*RB-09*");
        await _misionRepo.DidNotReceive().SaveAsync(Arg.Any<Mision>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConEtapaTriviaBorradorSinPreguntas_PersisteSinActivar()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        _categoriaRepo.FindByIdAsync(categoria.CategoriaId, Arg.Any<CancellationToken>()).Returns(categoria);

        Mision? guardada = null;
        _misionRepo
            .SaveAsync(Arg.Do<Mision>(m => guardada = m), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new CrearMisionCommand(
                "Misión borrador trivia",
                [new EtapaMisionInput("Trivia", 1, null, null, null, [categoria.CategoriaId.Valor])],
                Activar: false),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        guardada.Should().NotBeNull();
        guardada!.Estado.Should().Be(EstadoMision.Borrador);
        guardada.Etapas.Should().ContainSingle(e => e is EtapaTrivia);
    }

    [Fact]
    public async Task Handle_ConEtapaTriviaYActivarConPreguntas_ActivaYPersiste()
    {
        var categoria = TriviaTestBuilder.UnaCategoria();
        var pregunta = TriviaTestBuilder.PreguntaConCategoria(categoria.CategoriaId);
        _categoriaRepo.FindByIdAsync(categoria.CategoriaId, Arg.Any<CancellationToken>()).Returns(categoria);
        _preguntaRepo
            .FindByCategoriaAsync(categoria.CategoriaId, Arg.Any<CancellationToken>())
            .Returns([pregunta]);

        Mision? guardada = null;
        _misionRepo
            .SaveAsync(Arg.Do<Mision>(m => guardada = m), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(
            new CrearMisionCommand(
                "Misión trivia activa",
                [new EtapaMisionInput("Trivia", 1, null, null, null, [categoria.CategoriaId.Valor])],
                Activar: true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        guardada!.Estado.Should().Be(EstadoMision.Activa);
    }

    [Fact]
    public async Task Handle_CategoriaInexistente_LanzaNotFoundException()
    {
        var catId = Guid.NewGuid();
        _categoriaRepo.FindByIdAsync(Arg.Any<CategoriaId>(), Arg.Any<CancellationToken>())
            .Returns((Categoria?)null);

        var act = () => _sut.Handle(
            new CrearMisionCommand(
                "Sin cat",
                [new EtapaMisionInput("Trivia", 1, null, null, null, [catId])],
                Activar: false),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
