using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.Repositories;
using Umbral.Infrastructure.Tests.Support;

namespace Umbral.Infrastructure.Tests.Repositories;

[Collection(nameof(PostgresCollection))]
public sealed class SesionRepositoryTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SaveAsync_Y_FindByIdAsync_PersistenSesionConParticipantes()
    {
        var sesion = DomainTestData.SesionBusquedaTesoroActiva("Rangers");
        var sut = CreateRepository();

        await sut.SaveAsync(sesion);

        var loaded = await sut.FindByIdAsync(sesion.SesionId);

        loaded.Should().NotBeNull();
        loaded!.SesionId.Should().Be(sesion.SesionId);
        loaded.Estado.Should().Be(EstadoSesion.Activa);
        loaded.Participantes.Should().ContainSingle(e => e.Nombre.Valor == "Rangers");
    }

    [Fact]
    public async Task FindActivasAsync_RetornaSoloSesionesActivas()
    {
        var activa = DomainTestData.SesionBusquedaTesoroActiva("Activos");
        var programada = Sesion.CrearBusquedaTesoro(
            DomainTestData.MisionSnapshotActiva("Otra misión"),
            UsuarioId.Nuevo());
        programada.ClearDomainEvents();

        var sut = CreateRepository();
        await sut.SaveAsync(activa);
        await sut.SaveAsync(programada);

        var activas = await sut.FindActivasAsync();

        activas.Should().Contain(s => s.SesionId == activa.SesionId);
        activas.Should().NotContain(s => s.SesionId == programada.SesionId);
        activas.Should().OnlyContain(s => s.Estado == EstadoSesion.Activa);
    }

    private SesionRepository CreateRepository()
    {
        var db = fixture.CreateDbContext();
        return new SesionRepository(db);
    }
}
