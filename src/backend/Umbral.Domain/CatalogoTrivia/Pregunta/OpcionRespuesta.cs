using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoTrivia.Pregunta;

/// <summary>
/// Opción de respuesta de una pregunta de trivia (HU-24). VO inmutable.
/// </summary>
public sealed class OpcionRespuesta : ValueObject
{
    public string Texto { get; }
    public bool EsCorrecta { get; }

    private OpcionRespuesta(string texto, bool esCorrecta)
    {
        Texto = texto;
        EsCorrecta = esCorrecta;
    }

    public static OpcionRespuesta Crear(string texto, bool esCorrecta)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new DomainException("El texto de la opción no puede estar vacío.");

        return new OpcionRespuesta(texto.Trim(), esCorrecta);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Texto;
        yield return EsCorrecta;
    }
}
