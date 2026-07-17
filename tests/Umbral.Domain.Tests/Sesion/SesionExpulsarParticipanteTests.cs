using FluentAssertions;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;
using Umbral.Domain.Tests.Sesion.Builders;
using SesionAR = Umbral.Domain.Sesion.Sesion;
using Xunit;

namespace Umbral.Domain.Tests.Sesion;

/// <summary>HU-32 — ExpulsarParticipante (sala de espera).</summary>
public sealed class SesionExpulsarParticipanteTests
{
    [Fact]
    public void ExpulsarParticipante_CuandoEnPreparacion_EliminaParticipanteYRegistraMotivo()
    {
        var sesion = SesionEnPreparacion();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        var participanteId = sesion.Participantes.Single().ParticipanteId;

        var expulsado = sesion.ExpulsarParticipante(participanteId, "Nombre inapropiado");

        expulsado.Should().Be(participanteId);
        sesion.Participantes.Should().BeEmpty();
        sesion.HistorialEventos.Should().Contain(e =>
            e.Tipo == "ParticipanteExpulsado"
            && e.Payload.Contains("Alpha")
            && e.Payload.Contains("Nombre inapropiado"));
    }

    [Fact]
    public void ExpulsarParticipante_CuandoNoInscrito_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();

        var act = () => sesion.ExpulsarParticipante(ParticipanteId.Nuevo(), "Motivo");

        act.Should().Throw<DomainException>()
            .WithMessage("*no está inscrito*");
    }

    [Fact]
    public void ExpulsarParticipante_CuandoMotivoVacio_LanzaDomainException()
    {
        var sesion = SesionEnPreparacion();
        SesionTestHelpers.UnirParticipante(sesion, "Alpha");
        var participanteId = sesion.Participantes.Single().ParticipanteId;

        var act = () => sesion.ExpulsarParticipante(participanteId, "  ");

        act.Should().Throw<DomainException>()
            .WithMessage("*motivo*");
    }

    [Fact]
    public void ExpulsarParticipante_CuandoSesionActiva_LanzaDomainException()
    {
        var sesion = SesionBuilder.BusquedaTesoro().Activa().Build();
        var participanteId = sesion.Participantes.Single().ParticipanteId;

        var act = () => sesion.ExpulsarParticipante(participanteId, "Fuera de sala");

        act.Should().Throw<DomainException>()
            .WithMessage("*EnPreparacion*");
    }

    private static SesionAR SesionEnPreparacion() =>
        SesionBuilder.BusquedaTesoro()
            .ConEstado(EstadoSesion.EnPreparacion)
            .SinParticipantes()
            .Build();
}
