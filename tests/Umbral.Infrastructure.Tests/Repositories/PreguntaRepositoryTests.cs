using FluentAssertions;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Infrastructure.Persistence.Repositories;
using Umbral.Infrastructure.Tests.Support;

namespace Umbral.Infrastructure.Tests.Repositories;

[Collection(nameof(PostgresCollection))]
public sealed class PreguntaRepositoryTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SaveAsync_Y_FindByIdAsync_PersistenPreguntaConOpciones()
    {
        var pregunta = DomainTestData.PreguntaPersistenciaSinCategoria();
        var sut = CreateRepository();

        await sut.SaveAsync(pregunta);

        var loaded = await sut.FindByIdAsync(pregunta.PreguntaId);

        loaded.Should().NotBeNull();
        loaded!.Enunciado.Should().Be(pregunta.Enunciado);
        loaded.Dificultad.Should().Be(Dificultad.Facil);
        loaded.Opciones.Should().HaveCount(3);
        loaded.Opciones.Should().ContainSingle(o => o.EsCorrecta);
        loaded.Opciones.Should().Contain(o => o.Texto == "Opción correcta persistencia");
    }

    [Fact]
    public async Task FindByCategoriaAsync_RetornaPreguntasDeLaCategoria()
    {
        var categoria = DomainTestData.CategoriaPersistencia(
            $"Categoría FK persistencia {Guid.NewGuid():N}");
        var categoriaRepo = new CategoriaRepository(fixture.CreateDbContext());
        await categoriaRepo.SaveAsync(categoria);

        var conCategoria = DomainTestData.PreguntaPersistenciaConCategoria(categoria.CategoriaId);
        var sinCategoria = DomainTestData.PreguntaPersistenciaSinCategoria("Otra pregunta persistencia");

        var sut = CreateRepository();
        await sut.SaveAsync(conCategoria);
        await sut.SaveAsync(sinCategoria);

        var result = await sut.FindByCategoriaAsync(categoria.CategoriaId);

        result.Should().ContainSingle();
        result[0].PreguntaId.Should().Be(conCategoria.PreguntaId);
        result[0].CategoriaId.Should().Be(categoria.CategoriaId);
    }

    [Fact]
    public async Task SaveAsync_CuandoModificaOpciones_PersisteCambios()
    {
        var pregunta = DomainTestData.PreguntaPersistenciaSinCategoria();
        var sut = CreateRepository();
        await sut.SaveAsync(pregunta);

        var loaded = await sut.FindByIdAsync(pregunta.PreguntaId);
        loaded!.ModificarContenido(
            "Enunciado modificado persistencia",
            Dificultad.Dificil,
            [
                OpcionRespuesta.Crear("Nueva correcta persistencia", true),
                OpcionRespuesta.Crear("Nueva incorrecta 1 persistencia", false),
                OpcionRespuesta.Crear("Nueva incorrecta 2 persistencia", false)
            ]);

        await sut.SaveAsync(loaded);

        var reloaded = await sut.FindByIdAsync(pregunta.PreguntaId);
        reloaded!.Enunciado.Should().Be("Enunciado modificado persistencia");
        reloaded.Dificultad.Should().Be(Dificultad.Dificil);
        reloaded.Opciones.Should().Contain(o => o.Texto == "Nueva correcta persistencia");
    }

    private PreguntaRepository CreateRepository()
    {
        var db = fixture.CreateDbContext();
        return new PreguntaRepository(db);
    }
}
