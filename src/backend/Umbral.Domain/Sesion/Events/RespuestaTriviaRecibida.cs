using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

/// <summary>HU-34 — respuesta trivia recibida y evaluada (async consumer = HU-35).</summary>
public sealed record RespuestaTriviaRecibida(
    SesionId SesionId,
    ParticipanteId ParticipanteId,
    PreguntaId PreguntaId,
    int IndiceOpcion,
    bool EsCorrecta,
    bool FueraDeTiempo,
    int PuntosOtorgados) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
