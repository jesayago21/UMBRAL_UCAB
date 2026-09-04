using MediatR;

namespace Umbral.Application.Sesion.Queries.GetEstadoTriviaSesion;

public sealed record GetEstadoTriviaSesionQuery(Guid SesionId, Guid JugadorId)
    : IRequest<EstadoTriviaSesionDto>;

public sealed record EstadoTriviaSesionDto(
    string Fase,
    int? Orden,
    Guid? PreguntaId,
    string? Enunciado,
    string? Dificultad,
    IReadOnlyList<string>? Opciones,
    DateTime? TimerCerradoEnUtc,
    DateTime? TransicionHastaUtc,
    int PreguntaIndexActual,
    int TotalPreguntas,
    bool YaRespondio,
    int? IndiceOpcionSeleccionada,
    bool? UltimaRespuestaEsCorrecta = null,
    bool? UltimaRespuestaFueraDeTiempo = null,
    int? UltimaRespuestaPuntos = null,
    int? PuntajeTotalParticipante = null);
