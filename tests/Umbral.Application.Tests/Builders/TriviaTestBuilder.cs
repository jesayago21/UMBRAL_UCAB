using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Application.Tests.Builders;

/// <summary>
/// Datos de prueba para agregados de CatalogoTrivia.
/// Los valores por defecto son claramente ficticios; cada test pasa valores
/// explícitos cuando el escenario depende de ellos (filtros, unicidad, etc.).
/// </summary>
internal static class TriviaTestBuilder
{
    public const string NombreCategoriaDefault = "Categoría de prueba";
    public const string EnunciadoDefault = "Enunciado de pregunta de prueba";
    public const string EnunciadoConCategoriaDefault = "Enunciado de pregunta con categoría de prueba";

    public static Categoria UnaCategoria(string nombre = NombreCategoriaDefault)
    {
        var categoria = Categoria.Crear(nombre);
        categoria.ClearDomainEvents();
        return categoria;
    }

    public static Pregunta PreguntaSinCategoria(string enunciado = EnunciadoDefault)
    {
        var pregunta = Pregunta.Crear(
            enunciado,
            Dificultad.Facil,
            OpcionesValidas());
        pregunta.ClearDomainEvents();
        return pregunta;
    }

    public static Pregunta PreguntaConCategoria(CategoriaId categoriaId)
    {
        var pregunta = Pregunta.Crear(
            EnunciadoConCategoriaDefault,
            Dificultad.Media,
            OpcionesValidas(),
            categoriaId);
        pregunta.ClearDomainEvents();
        return pregunta;
    }

    public static IReadOnlyList<OpcionRespuesta> OpcionesValidas() =>
    [
        OpcionRespuesta.Crear("Opción correcta de prueba", true),
        OpcionRespuesta.Crear("Opción incorrecta 1 de prueba", false),
        OpcionRespuesta.Crear("Opción incorrecta 2 de prueba", false)
    ];
}
