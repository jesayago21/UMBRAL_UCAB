using FluentAssertions;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Infrastructure.Persistence.Repositories;
using Umbral.Infrastructure.Tests.Support;

namespace Umbral.Infrastructure.Tests.Repositories;

[Collection(nameof(PostgresCollection))]
public sealed class MisionRepositoryTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SaveAsync_Y_FindByIdAsync_PersistenMisionConEtapasYPistas()
    {
        var mision = DomainTestData.MisionConEtapasYPistas();
        var sut = CreateRepository();

        await sut.SaveAsync(mision);

        var loaded = await sut.FindByIdAsync(mision.MisionId);

        loaded.Should().NotBeNull();
        loaded!.Nombre.Should().Be(mision.Nombre);
        loaded.Etapas.Should().HaveCount(2);
        loaded.Etapas[0].Pistas.Should().ContainSingle(p => p.Contenido == "Pista por tiempo");
    }

    [Fact]
    public async Task FindActivasAsync_RetornaSoloMisionesActivas()
    {
        var activa = DomainTestData.MisionConEtapasYPistas("Activa");
        var borrador = Mision.Crear("Borrador");
        borrador.ClearDomainEvents();

        var sut = CreateRepository();
        await sut.SaveAsync(activa);
        await sut.SaveAsync(borrador);

        var activas = await sut.FindActivasAsync();

        activas.Should().Contain(m => m.MisionId == activa.MisionId);
        activas.Should().NotContain(m => m.MisionId == borrador.MisionId);
        activas.Should().OnlyContain(m => m.Estado == EstadoMision.Activa);
    }

    private MisionRepository CreateRepository()
    {
        var db = fixture.CreateDbContext();
        return new MisionRepository(db);
    }
}
