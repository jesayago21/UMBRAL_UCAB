namespace Umbral.Application.CatalogoTrivia.Models;

public sealed record CategoriaDto(Guid Id, string Nombre);

public sealed record OpcionRespuestaDto(string Texto, bool EsCorrecta);

public sealed record PreguntaDto(
    Guid Id,
    string Enunciado,
    string Dificultad,
    Guid? CategoriaId,
    IReadOnlyList<OpcionRespuestaDto> Opciones);

public sealed record OpcionRespuestaInput(string Texto, bool EsCorrecta);
