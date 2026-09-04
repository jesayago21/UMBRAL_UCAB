using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoMision.Mision.Events;
using Umbral.Domain.CatalogoTrivia.Categoria;
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
    public void EditarPistaEtapa_ActualizaContenidoYTipo()
    {
        var mision = Mision.Crear("Editar pista");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        var etapa = (EtapaBusquedaTesoro)mision.Etapas[0];
        etapa.AgregarPista("Original", TipoLiberacion.PorTiempo, 30);
        var pistaId = etapa.Pistas[0].PistaId;

        mision.EditarPistaEtapa(etapa.EtapaId, pistaId, "Actualizada", TipoLiberacion.PorGanador);

        etapa.Pistas.Should().ContainSingle(p =>
            p.PistaId == pistaId &&
            p.Contenido == "Actualizada" &&
            p.TipoLiberacion == TipoLiberacion.PorGanador &&
            p.SegundosLiberacion == null);
    }

    [Fact]
    public void EliminarPistaEtapa_RemuevePistaDelNodo()
    {
        var mision = Mision.Crear("Eliminar pista");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        var etapa = (EtapaBusquedaTesoro)mision.Etapas[0];
        etapa.AgregarPista("Pista A", TipoLiberacion.PorGanador);
        etapa.AgregarPista("Pista B", TipoLiberacion.PorTiempo, 45);
        var pistaId = etapa.Pistas[0].PistaId;

        mision.EliminarPistaEtapa(etapa.EtapaId, pistaId);

        etapa.Pistas.Should().ContainSingle(p => p.Contenido == "Pista B");
    }

    [Fact]
    public void EditarEtapaBusquedaTesoro_ActualizaDescripcionYQr()
    {
        var mision = Mision.Crear("Editar etapa BT");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        var etapaId = mision.Etapas[0].EtapaId;

        mision.EditarEtapaBusquedaTesoro(etapaId, "Patio", "QR-PATIO");

        var etapa = (EtapaBusquedaTesoro)mision.Etapas[0];
        etapa.Descripcion.Should().Be("Patio");
        etapa.CodigoQRSolucion.Should().Be("QR-PATIO");
    }

    [Fact]
    public void AgregarEtapaBusquedaTesoro_ConUbicacionCompleta_GuardaCoordsYRadio()
    {
        var mision = Mision.Crear("Con mapa");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001", 10.488, -66.847, 120);

        var etapa = (EtapaBusquedaTesoro)mision.Etapas[0];
        etapa.Latitud.Should().Be(10.488);
        etapa.Longitud.Should().Be(-66.847);
        etapa.RadioMetros.Should().Be(120);
    }

    [Fact]
    public void AgregarEtapaBusquedaTesoro_UbicacionIncompleta_LanzaDomainException()
    {
        var mision = Mision.Crear("Ubicacion incompleta");

        var act = () => mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001", 10.5, null, 100);

        act.Should().Throw<DomainException>().WithMessage("*latitud*longitud*radio*");
    }

    [Fact]
    public void AgregarEtapaBusquedaTesoro_RadioFueraDeRango_LanzaDomainException()
    {
        var mision = Mision.Crear("Radio inválido");

        var act = () => mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001", 10.5, -66.8, 5);

        act.Should().Throw<DomainException>().WithMessage("*radio*");
    }

    [Fact]
    public void EditarEtapaBusquedaTesoro_ActualizaUbicacion()
    {
        var mision = Mision.Crear("Editar mapa");
        mision.AgregarEtapaBusquedaTesoro("Hall", "QR-001");
        var etapaId = mision.Etapas[0].EtapaId;

        mision.EditarEtapaBusquedaTesoro(etapaId, "Hall", "QR-001", 10.49, -66.85, 150);

        var etapa = (EtapaBusquedaTesoro)mision.Etapas[0];
        etapa.Latitud.Should().Be(10.49);
        etapa.Longitud.Should().Be(-66.85);
        etapa.RadioMetros.Should().Be(150);
    }

    [Fact]
    public void EditarEtapaTrivia_ActualizaCategorias()
    {
        var catA = CategoriaId.Nuevo();
        var catB = CategoriaId.Nuevo();
        var mision = Mision.Crear("Editar etapa Trivia");
        mision.AgregarEtapaTrivia([catA]);
        var etapaId = mision.Etapas[0].EtapaId;

        mision.EditarEtapaTrivia(etapaId, [catA, catB]);

        var etapa = (EtapaTrivia)mision.Etapas[0];
        etapa.CategoriaIds.Should().HaveCount(2);
        etapa.CategoriaIds.Should().Contain(catB);
    }

    [Fact]
    public void AgregarEtapaTrivia_ConCategoriaYaUsadaEnOtraEtapa_LanzaDomainException()
    {
        var futbol = CategoriaId.Nuevo();
        var cultura = CategoriaId.Nuevo();
        var mision = Mision.Crear("Trivia multi-etapa");
        mision.AgregarEtapaTrivia([futbol, cultura]);

        var act = () => mision.AgregarEtapaTrivia([futbol]);

        act.Should().Throw<DomainException>()
            .WithMessage("*categoría de trivia no puede repetirse*");
    }

    [Fact]
    public void EditarEtapaTrivia_ConCategoriaDeOtraEtapa_LanzaDomainException()
    {
        var futbol = CategoriaId.Nuevo();
        var cultura = CategoriaId.Nuevo();
        var historia = CategoriaId.Nuevo();
        var mision = Mision.Crear("Editar solapamiento");
        mision.AgregarEtapaTrivia([futbol, cultura]);
        mision.AgregarEtapaTrivia([historia]);
        var segundaEtapaId = mision.Etapas[1].EtapaId;

        var act = () => mision.EditarEtapaTrivia(segundaEtapaId, [futbol]);

        act.Should().Throw<DomainException>()
            .WithMessage("*categoría de trivia no puede repetirse*");
    }

    [Fact]
    public void EliminarEtapa_ReordenaRestantes()
    {
        var mision = Mision.Crear("Eliminar etapa");
        mision.AgregarEtapaBusquedaTesoro("A", "QR-A");
        mision.AgregarEtapaBusquedaTesoro("B", "QR-B");
        var primera = mision.Etapas[0].EtapaId;

        mision.EliminarEtapa(primera);

        mision.Etapas.Should().ContainSingle();
        mision.Etapas[0].Orden.Should().Be(1);
        ((EtapaBusquedaTesoro)mision.Etapas[0]).Descripcion.Should().Be("B");
    }

    [Fact]
    public void EliminarEtapa_CuandoActivaConUnaSola_LanzaDomainException()
    {
        var mision = Mision.Crear("Activa única");
        mision.AgregarEtapaBusquedaTesoro("Única", "QR-1");
        mision.Activar();
        var etapaId = mision.Etapas[0].EtapaId;

        var act = () => mision.EliminarEtapa(etapaId);

        act.Should().Throw<DomainException>().WithMessage("*RB-09*");
    }

    [Fact]
    public void MisionId_ToString_DevuelveGuid()
    {
        var id = MisionId.Nuevo();

        id.ToString().Should().Be(id.Valor.ToString());
    }
}
