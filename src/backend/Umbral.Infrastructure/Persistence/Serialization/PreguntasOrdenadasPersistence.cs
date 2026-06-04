using System.Text.Json;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Infrastructure.Persistence.Serialization;

internal static class PreguntasOrdenadasPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string ToJson(IReadOnlyList<PreguntaId> preguntas) =>
        JsonSerializer.Serialize(preguntas.Select(p => p.Valor), JsonOptions);

    public static List<PreguntaId> FromJson(string json)
    {
        var ids = JsonSerializer.Deserialize<List<Guid>>(json, JsonOptions)
                  ?? [];

        return ids.Select(g => new PreguntaId(g)).ToList();
    }
}
