using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using SesionAR = Umbral.Domain.Sesion.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>
/// UnirseEquipo — jugador se une con código de acceso de la sesión.
/// </summary>
public sealed class SesionUnirseEquipoTests
{
    [Fact]
    public void UnirseEquipo_CuandoCodigoValido_AgregaEquipo()
    {
        var sesion = SesionEnPreparacion();
        var jugador = UsuarioId.Nuevo();

        var equipo = sesion.UnirseEquipo(jugador, "Alpha", sesion.CodigoAcceso.Valor);

        sesion.Equipos.Should().ContainSingle();
        equipo.Nombre.Valor.Should().Be("Alpha");
        equipo.JugadorId.Should().Be(jugador);
        equipo.SesionId.Should().Be(sesion.SesionId);
        equipo.EquipoId.Valor.Should().NotBe(Guid.Empty);
        equipo.PuntajeTotal.Valor.Should().Be(0);
    }

    [Fact]
    public void UnirseEquipo_CuandoCodigoIncorrecto_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();

        var act = () => sesion.UnirseEquipo(UsuarioId.Nuevo(), "Alpha", "CODIGO-INVALIDO");

        act.Should().Throw<DomainException>()
            .WithMessage("*código de acceso*");
    }

    [Fact]
    public void UnirseEquipo_CuandoJugadorDuplicado_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();
        var jugador = UsuarioId.Nuevo();
        SesionTestHelpers.UnirEquipo(sesion, "Alpha", jugador);

        var act = () => SesionTestHelpers.UnirEquipo(sesion, "Beta", jugador);

        act.Should().Throw<DomainException>()
            .WithMessage("*Ya estás inscrito*");
    }

    [Fact]
    public void UnirseEquipo_CuandoNombreDuplicado_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();
        SesionTestHelpers.UnirEquipo(sesion, "Alpha");

        var act = () => SesionTestHelpers.UnirEquipo(sesion, "alpha");

        act.Should().Throw<DomainException>()
            .WithMessage("*Ya existe un equipo*");
    }

    [Theory]
    [InlineData(EstadoSesion.Finalizada)]
    [InlineData(EstadoSesion.Cancelada)]
    public void UnirseEquipo_CuandoSesionCerrada_LanzaDomainException(EstadoSesion estadoCerrado)
    {
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(estadoCerrado)
            .Build();

        var act = () => SesionTestHelpers.UnirEquipo(sesion, "Alpha");

        act.Should().Throw<DomainException>()
            .WithMessage("*sesión cerrada*");
    }

    [Fact]
    public void UnirseEquipo_CuandoSesionActiva_LanzaDomainException()
    {
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();

        var act = () => SesionTestHelpers.UnirEquipo(sesion, "Beta");

        act.Should().Throw<DomainException>()
            .WithMessage("*ya está en juego*");
    }

    [Fact]
    public void UnirseEquipo_CuandoProgramada_AbreInscripcionYAgregaEquipo()
    {
        var sesion = SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.Programada)
            .SinEquipos()
            .Build();

        SesionTestHelpers.UnirEquipo(sesion, "Gamma");

        sesion.Estado.Should().Be(EstadoSesion.EnPreparacion);
        sesion.Equipos.Should().ContainSingle(e => e.Nombre.Valor == "Gamma");
        sesion.HistorialEventos.Should().Contain(e =>
            e.Tipo == "EquipoUnido" && e.Payload == "Gamma");
    }

    private static SesionAR SesionEnPreparacion() =>
        SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .SinEquipos()
            .Build();
}
