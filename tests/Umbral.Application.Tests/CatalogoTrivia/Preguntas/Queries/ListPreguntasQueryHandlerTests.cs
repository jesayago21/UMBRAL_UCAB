using FluentAssertions;
using NSubstitute;
using Umbral.Application.CatalogoTrivia.Preguntas.Queries.ListPreguntas;
using Umbral.Application.Tests.Builders;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Xunit;

namespace Umbral.Application.Tests.CatalogoTrivia.Preguntas.Queries;

public sealed class ListPreguntasQueryHandlerTests
{
    private readonly IPreguntaRepository _repo = Substitute.For<IPreguntaRepository>();
    private readonly ListPreguntasQueryHandler _sut;

    public ListPreguntasQueryHandlerTests()
        => _sut = new ListPreguntasQueryHandler(_repo);

    [Fact]
    public async Task Handle_FiltraPorCategoriaDificultadYEnunciado()
    {
        var categoriaId = CategoriaId.Nuevo();
        var facil = TriviaTestBuilder.PreguntaSinCategoria("Enunciado filtro capital de prueba");
        var dificil = Pregunta.Crear(
            "¿Teorema de Pitágoras?",
            Dificultad.Dificil,
            TriviaTestBuilder.OpcionesValidas(),
            categoriaId);
        dificil.ClearDomainEvents();
        var eliminada = TriviaTestBuilder.PreguntaSinCategoria("¿Otra?");
        eliminada.Eliminar();

        _repo.FindAllAsync(Arg.Any<CancellationToken>())
            .Returns([facil, dificil, eliminada]);

        var result = await _sut.Handle(
            new ListPreguntasQuery(null, "Facil", "capital"),
            CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Enunciado.Should().Contain("capital");
    }

    [Fact]
    public async Task Handle_ConCategoriaId_FiltraPorCategoria()
    {
        var categoriaId = CategoriaId.Nuevo();
        var conCategoria = TriviaTestBuilder.PreguntaConCategoria(categoriaId);
        var sinCategoria = TriviaTestBuilder.PreguntaSinCategoria();

        _repo.FindByCategoriaAsync(categoriaId, Arg.Any<CancellationToken>())
            .Returns([conCategoria]);

        var result = await _sut.Handle(
            new ListPreguntasQuery(categoriaId.Valor, null, null),
            CancellationToken.None);

        result.Should().ContainSingle();
        result[0].CategoriaId.Should().Be(categoriaId.Valor);
    }
}
