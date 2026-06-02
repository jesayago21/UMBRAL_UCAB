using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.Repositories;
using Umbral.Infrastructure.Tests.Support;

namespace Umbral.Infrastructure.Tests.Repositories;

[Collection(nameof(PostgresCollection))]
public sealed class ContextoBusquedaTesoroPersistenceTests(PostgresFixture fixture)
{
    [Fact]
    public async Task SaveAsync_Y_FindByIdAsync_PersisteContextoMisionConSnapshot()
    {
        var snapshot = DomainTestData.MisionSnapshotActiva("Misión contexto BT");
        var sesion     = Sesion.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.ClearDomainEvents();

        await using (var db = fixture.CreateDbContext())
        {
            var sut = new SesionRepository(db);
            await sut.SaveAsync(sesion);
        }

        await using var db2 = fixture.CreateDbContext();
        var loaded = await new SesionRepository(db2).FindByIdAsync(sesion.SesionId);

        loaded.Should().NotBeNull();
        loaded!.ContextoMision.Should().NotBeNull();
        loaded.ContextoMision!.MisionSnapshot.Nombre.Should().Be("Misión contexto BT");
        loaded.ContextoMision.MisionSnapshot.Etapas.Should().HaveCount(2);
        loaded.ContextoMision.EtapaActualIndex.Should().Be(0);
        loaded.ContextoMision.ObtenerEtapaBusquedaTesoroActual().CodigoQRSolucion.Should().Be("QR-ARBOL-001");
    }

    [Fact]
    public async Task SaveAsync_ActualizaEtapaIndexTrasAvanceEnDominio()
    {
        var sesion = DomainTestData.SesionBusquedaTesoroActiva("Ganadores");
        sesion.ClearDomainEvents();

        var participanteGanador = sesion.Participantes.First().ParticipanteId;
        var qrEtapa1      = sesion.ContextoMision!.ObtenerEtapaBusquedaTesoroActual().CodigoQRSolucion;
        sesion.RegistrarEvidencia(participanteGanador, qrEtapa1);

        await using (var db = fixture.CreateDbContext())
        {
            await new SesionRepository(db).SaveAsync(sesion);
        }

        await using var db2 = fixture.CreateDbContext();
        var loaded = await new SesionRepository(db2).FindByIdAsync(sesion.SesionId);

        loaded!.ContextoMision!.EtapaActualIndex.Should().Be(1);
        loaded.ContextoMision.ObtenerEtapaBusquedaTesoroActual().CodigoQRSolucion.Should().Be("QR-FUENTE-002");
    }

    [Fact]
    public async Task SaveAsync_PersistePistasEnSnapshotJson()
    {
        var mision = Mision.Crear("Misión con pistas");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-P-001");
        var etapaId = mision.Etapas.First().EtapaId;
        mision.AgregarPistaAEtapa(etapaId, "Busca cerca del árbol", TipoLiberacion.PorGanador, null);
        mision.Activar();
        var snapshot = MisionSnapshot.DesdeSoloBusquedaTesoro(mision);
        var sesion     = Sesion.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.ClearDomainEvents();

        await using (var db = fixture.CreateDbContext())
        {
            await new SesionRepository(db).SaveAsync(sesion);
        }

        await using var db2 = fixture.CreateDbContext();
        var loaded = await new SesionRepository(db2).FindByIdAsync(sesion.SesionId);

        var etapa0 = (EtapaBusquedaTesoroSnapshot)loaded!.ContextoMision!.MisionSnapshot.Etapas[0];
        etapa0.Pistas.Should().ContainSingle();
        etapa0.Pistas[0].Contenido.Should().Be("Busca cerca del árbol");
    }
}
