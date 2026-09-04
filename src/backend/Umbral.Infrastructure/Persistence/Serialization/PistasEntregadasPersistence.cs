using System.Text.Json;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.Serialization;

internal static class PistasEntregadasPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal sealed class PistaEntregadaDto
    {
        public Guid PistaId { get; init; }
        public int EtapaIndex { get; init; }
        public Guid ParticipanteId { get; init; }
        public DateTimeOffset EntregadaEn { get; init; }
        /// <summary>RF-15: texto ad-hoc; null = pista de catálogo.</summary>
        public string? Contenido { get; init; }
    }

    public static string ToJson(IReadOnlyList<PistaEntregada> items)
    {
        var dto = items.Select(p => new PistaEntregadaDto
        {
            PistaId        = p.PistaId.Valor,
            EtapaIndex     = p.EtapaIndex,
            ParticipanteId = p.ParticipanteId.Valor,
            EntregadaEn    = p.EntregadaEn,
            Contenido      = p.Contenido
        }).ToList();

        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public static List<PistaEntregada> FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        var dto = JsonSerializer.Deserialize<List<PistaEntregadaDto>>(json, JsonOptions)
                  ?? [];

        return dto
            .Select(p => PistaEntregada.Rehydrate(
                new PistaId(p.PistaId),
                p.EtapaIndex,
                new ParticipanteId(p.ParticipanteId),
                p.EntregadaEn,
                p.Contenido))
            .ToList();
    }
}
