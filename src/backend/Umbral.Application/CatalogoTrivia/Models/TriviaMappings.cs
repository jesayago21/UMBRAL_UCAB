using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Application.CatalogoTrivia.Models;

internal static class TriviaMappings
{
    public static CategoriaDto ToDto(this Categoria categoria) =>
        new(categoria.CategoriaId.Valor, categoria.Nombre);

    public static PreguntaDto ToDto(this Pregunta pregunta) =>
        new(
            pregunta.PreguntaId.Valor,
            pregunta.Enunciado,
            pregunta.Dificultad.ToString(),
            pregunta.CategoriaId?.Valor,
            pregunta.Opciones
                .Select(o => new OpcionRespuestaDto(o.Texto, o.EsCorrecta))
                .ToList());

    public static Dificultad ParseDificultad(string dificultad) =>
        Enum.Parse<Dificultad>(dificultad.Trim(), ignoreCase: true);

    public static IReadOnlyList<OpcionRespuesta> ToOpciones(
        IEnumerable<OpcionRespuestaInput> opciones) =>
        opciones
            .Select(o => OpcionRespuesta.Crear(o.Texto, o.EsCorrecta))
            .ToList();
}
