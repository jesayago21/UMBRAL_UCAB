using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoMision.Mision.Events;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.IdentidadYAccesos.Events;
using Umbral.Domain.Shared;
using Xunit;

namespace Umbral.Domain.Tests.CatalogoMision;

public sealed class MisionTests
{
    [Fact]
    public void Crear_ConNombreValido_EstadoBorradorYEventoMisionCreada()
    {
        var mision = Mision.Crear("  Misión campus  ");

        mision.Nombre.Should().Be("Misión campus");
        mision.Estado.Should().Be(EstadoMision.Borrador);
        mision.Etapas.Should().BeEmpty();
        mision.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<MisionCreada>();
    }

    [Fact]
    public void Crear_ConNombreVacio_LanzaDomainException()
    {
        var act = () => Mision.Crear("   ");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Activar_SinEtapas_LanzaDomainException()
    {
        var mision = Mision.Crear("Sin etapas");

        var act = () => mision.Activar();

        act.Should().Throw<DomainException>()
            .WithMessage("*RB-09*");
    }

    [Fact]
    public void Activar_DesdeBorradorConEtapas_EstadoActiva()
    {
        var mision = Mision.Crear("Activa");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");

        mision.Activar();

        mision.Estado.Should().Be(EstadoMision.Activa);
        mision.DomainEvents.OfType<MisionActivada>().Should().ContainSingle();
    }

    [Fact]
    public void Activar_CuandoNoBorrador_LanzaDomainException()
    {
        var mision = Mision.Crear("Ya activa");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        mision.Activar();
        mision.ClearDomainEvents();

        var act = () => mision.Activar();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Desactivar_CuandoActiva_VuelveABorrador()
    {
        var mision = Mision.Crear("Desactivar");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        mision.Activar();

        mision.Desactivar();

        mision.Estado.Should().Be(EstadoMision.Borrador);
    }

    [Fact]
    public void Desactivar_CuandoBorrador_LanzaDomainException()
    {
        var mision = Mision.Crear("Borrador");

        var act = () => mision.Desactivar();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Renombrar_ConNombreValido_ActualizaNombre()
    {
        var mision = Mision.Crear("Original");

        mision.Renombrar("  Nuevo nombre  ");

        mision.Nombre.Should().Be("Nuevo nombre");
    }

    [Fact]
    public void PuedeUsarseParaSesion_SoloCuandoActiva()
    {
        var mision = Mision.Crear("Estado");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");

        mision.PuedeUsarseParaSesion().Should().BeFalse();

        mision.Activar();

        mision.PuedeUsarseParaSesion().Should().BeTrue();
    }

    [Fact]
    public void AgregarEtapaTrivia_SinCategorias_LanzaDomainException()
    {
        var mision = Mision.Crear("Trivia vacía");

        var act = () => mision.AgregarEtapaTrivia([]);

        act.Should().Throw<DomainException>()
            .WithMessage("*RB-33*");
    }

    [Fact]
    public void AgregarEtapaTrivia_ConCategorias_DistinctIds()
    {
        var mision = Mision.Crear("Trivia");
        var catId = CategoriaId.Nuevo();

        mision.AgregarEtapaTrivia([catId, catId]);

        var trivia = mision.Etapas.Should().ContainSingle().Subject.Should().BeOfType<EtapaTrivia>().Subject;
        trivia.CategoriaIds.Should().ContainSingle().Which.Should().Be(catId);
        trivia.Tipo.Should().Be(TipoEtapa.Trivia);
    }

    [Fact]
    public void AgregarEtapaBusquedaTesoro_AgregaPistaYExponeTipo()
    {
        var mision = Mision.Crear("BT");
        mision.AgregarEtapaBusquedaTesoro("Hall principal", "QR-HALL");

        var bt = mision.Etapas.Should().ContainSingle().Subject.Should().BeOfType<EtapaBusquedaTesoro>().Subject;
        bt.Tipo.Should().Be(TipoEtapa.BusquedaTesoro);
        bt.AgregarPista("Busca el mural", TipoLiberacion.PorTiempo, 60);
        bt.Pistas.Should().ContainSingle(p => p.Contenido == "Busca el mural");
    }

    [Fact]
    public void AgregarPistaAEtapa_CuandoEtapaNoBt_LanzaDomainException()
    {
        var mision = Mision.Crear("Mixta");
        mision.AgregarEtapaTrivia([CategoriaId.Nuevo()]);
        var etapaId = mision.Etapas[0].EtapaId;

        var act = () => mision.AgregarPistaAEtapa(etapaId, "Pista", TipoLiberacion.PorGanador);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MisionId_ToString_DevuelveGuid()
    {
        var id = MisionId.Nuevo();

        id.ToString().Should().Be(id.Valor.ToString());
    }
}
