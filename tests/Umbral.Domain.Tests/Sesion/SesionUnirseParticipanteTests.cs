using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using SesionAR = Umbral.Domain.Sesion.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// UnirseParticipante — jugador se une con código de acceso de la sesión.
/// </summary>
public sealed class SesionUnirseParticipanteTests
{
    [Fact]
    public void UnirseParticipante_CuandoCodigoValido_AgregaParticipante()
    {
        var sesion = SesionEnPreparacion();
        var jugador = UsuarioId.Nuevo();

        var participante = sesion.UnirseParticipante(jugador, "Alpha", sesion.CodigoAcceso.Valor);

        sesion.Participantes.Should().ContainSingle();
        participante.Nombre.Valor.Should().Be("Alpha");
        participante.JugadorId.Should().Be(jugador);
        participante.SesionId.Should().Be(sesion.SesionId);
        participante.ParticipanteId.Valor.Should().NotBe(Guid.Empty);
        participante.PuntajeTotal.Valor.Should().Be(0);
    }

    [Fact]
    public void UnirseParticipante_CuandoCodigoIncorrecto_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();

        var act = () => sesion.UnirseParticipante(UsuarioId.Nuevo(), "Alpha", "CODIGO-INVALIDO");

        act.Should().Throw<DomainException>()
            .WithMessage("*código de acceso*");
    }

    [Fact]
    public void UnirseParticipante_CuandoJugadorDuplicado_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();
        var jugador = UsuarioId.Nuevo();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha", jugador);

        var act = () => SesionTestHelpers.UnirParticipante(sesion, "Beta", jugador);

        act.Should().Throw<DomainException>()
            .WithMessage("*Ya estás inscrito*");
    }

    [Fact]
    public void UnirseParticipante_CuandoNombreDuplicado_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");

        var act = () => SesionTestHelpers.UnirParticipante(sesion, "alpha");

        act.Should().Throw<DomainException>()
            .WithMessage("*Ya existe un participante*");
    }

    [Theory]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void UnirseParticipante_CuandoSesionCerrada_LanzaDomainException(EstadoSesion estadoCerrado)
    {
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(estadoCerrado)
            .Build();

        var act = () => SesionTestHelpers.UnirParticipante(sesion, "Alpha");

        act.Should().Throw<DomainException>()
            .WithMessage("*sesión cerrada*");
    }

    [Fact]
    public void UnirseParticipante_CuandoSesionActiva_LanzaDomainException()
    {
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        var act = () => SesionTestHelpers.UnirParticipante(sesion, "Beta");

        act.Should().Throw<DomainException>()
            .WithMessage("*ya está en juego*");
    }

    [Fact]
    public void UnirseParticipante_CuandoProgramada_AbreInscripcionYAgregaParticipante()
    {
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.Programada)
            .SinParticipantes()
            .Build();

        SesionTestHelpers.UnirParticipante(sesion, "Gamma");

        sesion.Estado.Should().Be(EstadoSesion.EnPreparacion);
        sesion.Participantes.Should().ContainSingle(e => e.Nombre.Valor == "Gamma");
        sesion.HistorialEventos.Should().Contain(e =>
            e.Tipo == "ParticipanteUnido" && e.Payload == "Gamma");
    }

    [Fact]
    public void UnirseParticipante_CuandoCupoCompleto_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();
        for (var i = 1; i <= SesionAR.MaxParticipantes; i++)
            SesionTestHelpers.UnirParticipante(sesion, $"P{i}");

        var act = () => SesionTestHelpers.UnirParticipante(sesion, "Extra");

        act.Should().Throw<DomainException>()
            .WithMessage("*máximo de*participantes*");
        sesion.Participantes.Should().HaveCount(SesionAR.MaxParticipantes);
    }

    private static SesionAR SesionEnPreparacion() =>
        SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .SinParticipantes()
            .Build();
}
