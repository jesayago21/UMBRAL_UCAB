using FluentAssertions;
using Umbral.Domain.Ports;
using Umbral.Infrastructure.Messaging.Publishers;
using Xunit;

namespace Umbral.Infrastructure.Tests.Messaging;

public sealed class NoOpNotificacionRealTimeTests
{
    private readonly NoOpNotificacionRealTime _sut = new();

    [Fact]
    public async Task TodasLasNotificaciones_CompletanSinError()
    {
        await _sut.NotificarCambioEstadoSesionAsync("s1", "Activa");
        await _sut.NotificarParticipantesActualizadosAsync("s1", 2);
        await _sut.NotificarPistaLiberadaAsync("s1", "p1", "pi1", 0, "contenido");
        await _sut.NotificarEtapaAvanzadaAsync("s1", 1, "Trivia");
        await _sut.NotificarRankingActualizadoAsync(
            "s1",
            [new RankingPosicionNotificacion(Guid.NewGuid(), "Alpha", 100, 1)]);
        await _sut.NotificarPenalizacionAplicadaAsync("s1", "p1", 10, "motivo");
        await _sut.NotificarParticipanteExpulsadoAsync("s1", "p1", "motivo");
        await _sut.NotificarPreguntaTriviaIniciadaAsync(
            "s1",
            Guid.NewGuid().ToString(),
            1,
            "¿Pregunta?",
            ["A", "B", "C"],
            DateTime.UtcNow.AddSeconds(30),
            3);
        await _sut.NotificarTriviaEnTransicionAsync("s1", 1, 3);

        true.Should().BeTrue();
    }
}
