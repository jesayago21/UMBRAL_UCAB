using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using SesionAR = Umbral.Domain.Sesion.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

public sealed class SesionAbandonarParticipanteTests
{
    [Fact]
    public void AbandonarParticipante_CuandoInscritoYEnPreparacion_EliminaParticipante()
    {
        var jugador = UsuarioId.Nuevo();
        var sesion = SesionEnPreparacion();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha", jugador);

        var participanteId = sesion.AbandonarParticipante(jugador);

        participanteId.Valor.Should().NotBeEmpty();
        sesion.Participantes.Should().BeEmpty();
        sesion.HistorialEventos.Should().Contain(e =>
            e.Tipo == "ParticipanteAbandono" && e.Payload == "Alpha");
    }

    [Fact]
    public void AbandonarParticipante_CuandoNoInscrito_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();

        var act = () => sesion.AbandonarParticipante(UsuarioId.Nuevo());

        act.Should().Throw<DomainException>()
            .WithMessage("*No estás inscrito*");
    }

    [Fact]
    public void AbandonarParticipante_CuandoSesionActiva_LanzaDomainException()
    {
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var jugador = sesion.Participantes.Single().JugadorId;

        var act = () => sesion.AbandonarParticipante(jugador);

        act.Should().Throw<DomainException>()
            .WithMessage("*en juego*");
    }

    private static SesionAR SesionEnPreparacion() =>
        SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .SinParticipantes()
            .Build();
}
