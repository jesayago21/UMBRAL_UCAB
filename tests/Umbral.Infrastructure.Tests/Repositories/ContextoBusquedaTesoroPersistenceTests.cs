using FluentAssertions;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.Repositories;
using Umbral.Infrastructure.Tests.Support;

namespace Umbral.Infrastructure.Tests.Repositories;

[Collection(nameof(PostgresCollection))]
public sealed class ContextoBusquedaTesoroPersistenceTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SaveAsync_Y_FindByIdAsync_PersisteContextoBTConSnapshot()
    {
        var snapshot = DomainTestData.MisionSnapshotActiva("Misión contexto BT");
        var sesion     = Sesion.CrearBusquedaTesoro(snapshot, UsuarioId.Nuevo());
        sesion.ClearDomainEvents();

        await using (var db = fixture.CreateDbContext())
        {
            var sut = new SesionRepository(db);
            await sut.SaveAsync(sesion);
        }

        await using var db2 = fixture.CreateDbContext();
        var loaded = await new SesionRepository(db2).FindByIdAsync(sesion.SesionId);

        loaded.Should().NotBeNull();
        loaded!.ContextoBT.Should().NotBeNull();
        loaded.ContextoBT!.MisionSnapshot.Nombre.Should().Be("Misión contexto BT");
        loaded.ContextoBT.MisionSnapshot.Etapas.Should().HaveCount(2);
        loaded.ContextoBT.EtapaActualIndex.Should().Be(0);
        loaded.ContextoBT.ObtenerEtapaActual().CodigoQRSolucion.Should().Be("QR-ARBOL-001");
    }

    [Fact]
    public async Task SaveAsync_ActualizaEtapaIndexTrasAvanceEnDominio()
    {
        var sesion = DomainTestData.SesionBusquedaTesoroActiva("Ganadores");
        sesion.ClearDomainEvents();

        var equipoGanador = sesion.Equipos.First().EquipoId;
        var qrEtapa1      = sesion.ContextoBT!.ObtenerEtapaActual().CodigoQRSolucion;
        sesion.RegistrarEvidencia(equipoGanador, qrEtapa1);

        await using (var db = fixture.CreateDbContext())
        {
            await new SesionRepository(db).SaveAsync(sesion);
        }

        await using var db2 = fixture.CreateDbContext();
        var loaded = await new SesionRepository(db2).FindByIdAsync(sesion.SesionId);

        loaded!.ContextoBT!.EtapaActualIndex.Should().Be(1);
        loaded.ContextoBT.ObtenerEtapaActual().CodigoQRSolucion.Should().Be("QR-FUENTE-002");
    }

    [Fact]
    public async Task SaveAsync_PersistePistasEnSnapshotJson()
    {
        var mision = Mision.Crear("Misión con pistas");
        mision.AgregarEtapa("Etapa 1", "QR-P-001");
        var etapaId = mision.Etapas.First().EtapaId;
        mision.AgregarPistaAEtapa(etapaId, "Busca cerca del árbol", TipoLiberacion.PorGanador, null);
        mision.Activar();
        var snapshot = MisionSnapshot.Desde(mision);
        var sesion     = Sesion.CrearBusquedaTesoro(snapshot, UsuarioId.Nuevo());
        sesion.ClearDomainEvents();

        await using (var db = fixture.CreateDbContext())
        {
            await new SesionRepository(db).SaveAsync(sesion);
        }

        await using var db2 = fixture.CreateDbContext();
        var loaded = await new SesionRepository(db2).FindByIdAsync(sesion.SesionId);

        loaded!.ContextoBT!.MisionSnapshot.Etapas[0].Pistas.Should().ContainSingle();
        loaded.ContextoBT.MisionSnapshot.Etapas[0].Pistas[0].Contenido.Should().Be("Busca cerca del árbol");
    }
}
