using FluentAssertions;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Infrastructure.Persistence.Repositories;
using Umbral.Infrastructure.Tests.Support;

namespace Umbral.Infrastructure.Tests.Repositories;

[Collection(nameof(PostgresCollection))]
public sealed class CategoriaRepositoryTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SaveAsync_Y_FindByIdAsync_PersistenCategoria()
    {
        var categoria = DomainTestData.CategoriaPersistencia();
        var sut = CreateRepository();

        await sut.SaveAsync(categoria);

        var loaded = await sut.FindByIdAsync(categoria.CategoriaId);

        loaded.Should().NotBeNull();
        loaded!.Nombre.Should().Be(categoria.Nombre);
        loaded.Eliminada.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNombreAsync_CuandoNombreDuplicadoActivo_RetornaTrue()
    {
        var existente = DomainTestData.CategoriaPersistencia("Categoría única persistencia");
        var sut = CreateRepository();
        await sut.SaveAsync(existente);

        var exists = await sut.ExistsByNombreAsync("Categoría única persistencia");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNombreAsync_CuandoCategoriaEliminada_NoCuentaParaUnicidad()
    {
        var eliminada = DomainTestData.CategoriaPersistencia("Categoría reutilizable persistencia");
        eliminada.Eliminar();
        var sut = CreateRepository();
        await sut.SaveAsync(eliminada);

        var exists = await sut.ExistsByNombreAsync("Categoría reutilizable persistencia");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FindAllAsync_RetornaTodasLasCategorias()
    {
        var a = DomainTestData.CategoriaPersistencia("Categoría A persistencia");
        var b = DomainTestData.CategoriaPersistencia("Categoría B persistencia");
        var sut = CreateRepository();
        await sut.SaveAsync(a);
        await sut.SaveAsync(b);

        var all = await sut.FindAllAsync();

        all.Should().Contain(c => c.CategoriaId == a.CategoriaId);
        all.Should().Contain(c => c.CategoriaId == b.CategoriaId);
    }

    private CategoriaRepository CreateRepository()
    {
        var db = fixture.CreateDbContext();
        return new CategoriaRepository(db);
    }
}
