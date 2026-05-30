using System.Text.Json;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Infrastructure.Persistence.Serialization;

internal static class OpcionesRespuestaPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal sealed record OpcionJson(string Texto, bool EsCorrecta);

    public static string ToJson(IReadOnlyList<OpcionRespuesta> opciones)
    {
        var payload = opciones
            .Select(o => new OpcionJson(o.Texto, o.EsCorrecta))
            .ToList();

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    public static List<OpcionRespuesta> FromJson(string json)
    {
        var payload = JsonSerializer.Deserialize<List<OpcionJson>>(json, JsonOptions)
            ?? [];

        return payload
            .Select(o => OpcionRespuesta.Crear(o.Texto, o.EsCorrecta))
            .ToList();
    }
}
