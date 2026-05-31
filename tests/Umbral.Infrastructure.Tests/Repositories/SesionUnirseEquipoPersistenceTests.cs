using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.Repositories;
using Umbral.Infrastructure.Tests.Support;

namespace Umbral.Infrastructure.Tests.Repositories;

[Collection(nameof(PostgresCollection))]
public sealed class SesionUnirseEquipoPersistenceTests(PostgresFixture fixture)
{
    [Fact]
    public async Task CrearSesionYUnirseEquipo_EnDosPasos_PersisteEquipo()
    {
        var sesion = Sesion.CrearBusquedaTesoro(
            DomainTestData.MisionSnapshotActiva(),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();

        await using (var db = fixture.CreateDbContext())
        {
            var sut = new SesionRepository(db);
            await sut.SaveAsync(sesion);
        }

        await using var db2 = fixture.CreateDbContext();
        var sut2 = new SesionRepository(db2);
        var loaded = await sut2.FindByIdAsync(sesion.SesionId);

        loaded.Should().NotBeNull();
        loaded!.UnirseEquipo(
            UsuarioId.Nuevo(),
            "Beta",
            loaded.CodigoAcceso.Valor);
        await sut2.SaveAsync(loaded);

        await using var verifyDb = fixture.CreateDbContext();
        var reloaded = await verifyDb.EquiposSesion
            .AsNoTracking()
            .Where(e => e.SesionId == sesion.SesionId)
            .ToListAsync();

        reloaded.Should().ContainSingle(e => e.Nombre.Valor == "Beta");
    }
}
