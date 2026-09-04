namespace Umbral.API.Contracts.Sesiones;

public sealed record LanzarPreguntaTriviaRequest(int? DuracionSegundos);

public sealed record EstadoTriviaSesionResponse(
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

public sealed record SubmitRespuestaTriviaRequest(
    Guid PreguntaId,
    int IndiceOpcion,
    int? DuracionTimerSegundos = null);

/// <summary>202 Accepted — respuesta encolada (HU-35).</summary>
public sealed record SubmitRespuestaTriviaResponse(
    Guid MessageId,
    string Status);
