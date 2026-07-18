using FluentAssertions;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Sesion;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using Xunit;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>HU-09 — Liberación automática PorTiempo (RB-07, RB-21, RB-23).</summary>
public sealed class SesionLiberarPistasPorTiempoTests
{
    [Fact]
    public void LiberarPistasPorTiempoVencidas_CuandoTiempoVencido_EntregaATodosLosParticipantes()
    {
        var (sesion, pistaId) = CrearSesionActivaConPistaPorTiempo(segundos: 30);
        var ahora = DateTimeOffset.UtcNow.AddSeconds(31);

        var count = sesion.LiberarPistasPorTiempoVencidas(ahora);

        count.Should().Be(2);
        sesion.ContextoMision!.PistasEntregadas.Should().HaveCount(2);
        sesion.DomainEvents.OfType<PistaLiberada>().Should().HaveCount(2);
        sesion.DomainEvents.OfType<PistaLiberada>()
            .Select(e => e.PistaId)
            .Should().OnlyContain(id => id == pistaId);
        sesion.HistorialEventos.Should().Contain(e => e.Tipo == "PistaLiberada");
    }

    [Fact]
    public void LiberarPistasPorTiempoVencidas_CuandoTiempoNoVencido_NoEntrega()
    {
        var (sesion, _) = CrearSesionActivaConPistaPorTiempo(segundos: 60);
        var ahora = DateTimeOffset.UtcNow.AddSeconds(10);

        var count = sesion.LiberarPistasPorTiempoVencidas(ahora);

        count.Should().Be(0);
        sesion.ContextoMision!.PistasEntregadas.Should().BeEmpty();
        sesion.DomainEvents.OfType<PistaLiberada>().Should().BeEmpty();
    }

    [Fact]
    public void LiberarPistasPorTiempoVencidas_CuandoPausada_NoEntrega()
    {
        var (sesion, _) = CrearSesionActivaConPistaPorTiempo(segundos: 10);
        sesion.Pausar();
        sesion.ClearDomainEvents();
        var ahora = DateTimeOffset.UtcNow.AddSeconds(60);

        var count = sesion.LiberarPistasPorTiempoVencidas(ahora);

        count.Should().Be(0);
        sesion.ContextoMision!.PistasEntregadas.Should().BeEmpty();
    }

    [Fact]
    public void LiberarPistasPorTiempoVencidas_SegundaInvocacion_NoDuplicaEntrega()
    {
        var (sesion, _) = CrearSesionActivaConPistaPorTiempo(segundos: 5);
        var ahora = DateTimeOffset.UtcNow.AddSeconds(10);

        sesion.LiberarPistasPorTiempoVencidas(ahora).Should().Be(2);
        sesion.ClearDomainEvents();

        var segunda = sesion.LiberarPistasPorTiempoVencidas(ahora.AddSeconds(5));

        segunda.Should().Be(0);
        sesion.ContextoMision!.PistasEntregadas.Should().HaveCount(2);
        sesion.DomainEvents.OfType<PistaLiberada>().Should().BeEmpty();
    }

    [Fact]
    public void LiberarPistasPorTiempoVencidas_CuandoEtapaTrivia_NoEntrega()
    {
        var sesion = CrearSesionActivaConEtapaTrivia();
        var ahora = DateTimeOffset.UtcNow.AddHours(1);

        var count = sesion.LiberarPistasPorTiempoVencidas(ahora);

        count.Should().Be(0);
    }

    [Fact]
    public void LiberarPistasPorTiempoVencidas_CuandoFinalizada_NoEntrega()
    {
        var (sesion, _) = CrearSesionActivaConPistaPorTiempo(segundos: 5);
        sesion.Finalizar();
        sesion.ClearDomainEvents();

        var count = sesion.LiberarPistasPorTiempoVencidas(DateTimeOffset.UtcNow.AddSeconds(60));

        count.Should().Be(0);
    }

    [Fact]
    public void LiberarPistasPorTiempoVencidas_TrasPausaYReanudar_ExcluyeTiempoPausado()
    {
        var (sesion, _) = CrearSesionActivaConPistaPorTiempo(segundos: 30);
        var t0 = sesion.ContextoMision!.EtapaIniciadaEn!.Value;

        // 20s activos → 40s en pausa → reanudar. A wall-clock t0+60 solo hay 20s efectivos (< 30).
        // RegistrarPausa/Reanudacion con instantes controlados (Pausar/Reanudar usan UtcNow).
        sesion.ContextoMision.RegistrarPausa(t0.AddSeconds(20));
        sesion.ContextoMision.RegistrarReanudacion(t0.AddSeconds(60));

        var countTemprano = sesion.LiberarPistasPorTiempoVencidas(t0.AddSeconds(60));
        countTemprano.Should().Be(0);
        sesion.ContextoMision.PistasEntregadas.Should().BeEmpty();

        // 75 wall − 40 pausa = 35s efectivos ≥ 30 → libera.
        var countVencido = sesion.LiberarPistasPorTiempoVencidas(t0.AddSeconds(75));
        countVencido.Should().Be(2);
    }

    [Fact]
    public void LiberarPistasPorTiempoVencidas_CuandoElapsedExacto_Libera()
    {
        var (sesion, _) = CrearSesionActivaConPistaPorTiempo(segundos: 30);
        var t0 = sesion.ContextoMision!.EtapaIniciadaEn!.Value;

        var count = sesion.LiberarPistasPorTiempoVencidas(t0.AddSeconds(30));

        count.Should().Be(2);
    }

    [Fact]
    public void LiberarPistasPorTiempoVencidas_CuandoSoloAlInicio_NoVuelveAEntregar()
    {
        var mision = Mision.Crear("Solo AlInicio");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-PG-001");
        ((EtapaBusquedaTesoro)mision.Etapas[0])
            .AgregarPista("Pista inicial", TipoLiberacion.PorGanador);
        mision.Activar();
        mision.ClearDomainEvents();

        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(mision),
            UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        sesion.Iniciar();
        sesion.ClearDomainEvents();

        // Ya se entregó al iniciar; PorTiempo no debe duplicar ni tocar AlInicio.
        var count = sesion.LiberarPistasPorTiempoVencidas(DateTimeOffset.UtcNow.AddHours(1));

        count.Should().Be(0);
        sesion.ContextoMision!.PistasEntregadas.Should().ContainSingle();
    }

    [Fact]
    public void Iniciar_EnEtapaBt_MarcaEtapaIniciadaEn()
    {
        var mision = CrearMisionConPista(30);
        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(mision),
            UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");

        sesion.Iniciar();

        sesion.ContextoMision!.EtapaIniciadaEn.Should().NotBeNull();
    }

    private static (SesionAR Sesion, PistaId PistaId) CrearSesionActivaConPistaPorTiempo(int segundos)
    {
        var mision = CrearMisionConPista(segundos);
        var pistaId = ((EtapaBusquedaTesoro)mision.Etapas[0]).Pistas[0].PistaId;

        var sesion = SesionAR.CrearDesdeMision(
            MisionSnapshot.DesdeSoloBusquedaTesoro(mision),
            UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        sesion.AbrirParaRegistro();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        SesionTestHelpers.UnirParticipante(sesion, "Beta");
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return (sesion, pistaId);
    }

    private static Mision CrearMisionConPista(int segundos)
    {
        var mision = Mision.Crear("Misión PorTiempo");
        mision.AgregarEtapaBusquedaTesoro("Etapa 1", "QR-PT-001");
        ((EtapaBusquedaTesoro)mision.Etapas[0])
            .AgregarPista("Pista reloj", TipoLiberacion.PorTiempo, segundos);
        mision.Activar();
        mision.ClearDomainEvents();
        return mision;
    }

    private static SesionAR CrearSesionActivaConEtapaTrivia()
    {
        var mision = Mision.Crear("Solo trivia");
        mision.AgregarEtapaTrivia([Umbral.Domain.CatalogoTrivia.Categoria.CategoriaId.Nuevo()]);
        var trivia = (EtapaTrivia)mision.Etapas[0];
        var snap = EtapaTriviaSnapshot.DesdeEtapa(
            trivia,
            [Umbral.Domain.CatalogoTrivia.Pregunta.PreguntaId.Nuevo()],
            "Trivia");
        var snapshot = MisionSnapshot.Desde(
            mision,
            new Dictionary<EtapaId, EtapaTriviaSnapshot> { [trivia.EtapaId] = snap });

        var sesion = SesionAR.CrearDesdeMision(snapshot, UsuarioId.Nuevo());
        sesion.AbrirParaRegistro();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }
}
