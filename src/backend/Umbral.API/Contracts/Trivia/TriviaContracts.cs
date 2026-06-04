namespace Umbral.API.Contracts.Trivia;

public sealed record CrearCategoriaRequest(string Nombre);

public sealed record ActualizarCategoriaRequest(string Nombre);

public sealed record CategoriaResponse(Guid Id, string Nombre);

public sealed record OpcionRespuestaRequest(string Texto, bool EsCorrecta);

public sealed record CrearPreguntaRequest(
    string Enunciado,
    string Dificultad,
    Guid? CategoriaId,
    IReadOnlyList<OpcionRespuestaRequest> Opciones);

public sealed record ActualizarPreguntaRequest(
    string Enunciado,
    string Dificultad,
    Guid? CategoriaId,
    IReadOnlyList<OpcionRespuestaRequest> Opciones);

public sealed record OpcionRespuestaResponse(string Texto, bool EsCorrecta);

public sealed record PreguntaResponse(
    Guid Id,
    string Enunciado,
    string Dificultad,
    Guid? CategoriaId,
    IReadOnlyList<OpcionRespuestaResponse> Opciones);
