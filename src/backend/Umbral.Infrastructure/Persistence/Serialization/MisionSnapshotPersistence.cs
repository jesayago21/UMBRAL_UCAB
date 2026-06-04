using System.Text.Json;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Infrastructure.Persistence.Serialization;

internal static class MisionSnapshotPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal sealed class MisionSnapshotDto
    {
        public Guid MisionId { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public List<EtapaSnapshotDto> Etapas { get; init; } = [];
    }

    internal sealed class EtapaSnapshotDto
    {
        public string Tipo { get; init; } = "BusquedaTesoro";
        public Guid EtapaId { get; init; }
        public int Orden { get; init; }
        public string? Descripcion { get; init; }
        public string? CodigoQRSolucion { get; init; }
        public List<PistaSnapshotDto>? Pistas { get; init; }
        public List<Guid>? CategoriaIds { get; init; }
        public List<Guid>? PreguntasOrdenadas { get; init; }
        public string? CategoriasTitulo { get; init; }
    }

    internal sealed class PistaSnapshotDto
    {
        public string Contenido { get; init; } = string.Empty;
        public string TipoLiberacion { get; init; } = string.Empty;
        public int? SegundosLiberacion { get; init; }
    }

    public static string ToJson(MisionSnapshot snapshot)
    {
        var dto = new MisionSnapshotDto
        {
            MisionId = snapshot.MisionId.Valor,
            Nombre   = snapshot.Nombre,
            Etapas   = snapshot.Etapas.Select(MapEtapa).ToList()
        };

        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public static MisionSnapshot FromJson(string json)
    {
        var dto = JsonSerializer.Deserialize<MisionSnapshotDto>(json, JsonOptions)
                  ?? throw new InvalidOperationException("JSON de MisionSnapshot inválido.");

        var etapas = dto.Etapas.Select(MapEtapaFromDto).Cast<EtapaSnapshotBase>().ToList().AsReadOnly();

        return MisionSnapshot.Rehydrate(
            new MisionId(dto.MisionId),
            dto.Nombre,
            etapas);
    }

    private static EtapaSnapshotDto MapEtapa(EtapaSnapshotBase e) => e switch
    {
        EtapaBusquedaTesoroSnapshot bt => new EtapaSnapshotDto
        {
            Tipo             = "BusquedaTesoro",
            EtapaId          = bt.EtapaId.Valor,
            Orden            = bt.Orden,
            Descripcion      = bt.Descripcion,
            CodigoQRSolucion = bt.CodigoQRSolucion,
            Pistas           = bt.Pistas.Select(p => new PistaSnapshotDto
            {
                Contenido          = p.Contenido,
                TipoLiberacion     = p.TipoLiberacion.ToString(),
                SegundosLiberacion = p.SegundosLiberacion
            }).ToList()
        },
        EtapaTriviaSnapshot trivia => new EtapaSnapshotDto
        {
            Tipo               = "Trivia",
            EtapaId            = trivia.EtapaId.Valor,
            Orden              = trivia.Orden,
            CategoriaIds       = trivia.CategoriaIds.Select(c => c.Valor).ToList(),
            PreguntasOrdenadas = trivia.PreguntasOrdenadas.Select(p => p.Valor).ToList(),
            CategoriasTitulo   = trivia.CategoriasTitulo
        },
        _ => throw new InvalidOperationException($"Tipo de etapa snapshot no soportado: {e.GetType().Name}")
    };

    private static EtapaSnapshotBase MapEtapaFromDto(EtapaSnapshotDto e)
    {
        if (string.Equals(e.Tipo, "Trivia", StringComparison.OrdinalIgnoreCase))
        {
            return EtapaTriviaSnapshot.Rehydrate(
                new EtapaId(e.EtapaId),
                e.Orden,
                (e.CategoriaIds ?? []).Select(g => new CategoriaId(g)).ToList().AsReadOnly(),
                (e.PreguntasOrdenadas ?? []).Select(g => new PreguntaId(g)).ToList().AsReadOnly(),
                e.CategoriasTitulo ?? "Trivia");
        }

        var pistas = (e.Pistas ?? [])
            .Select(p => PistaSnapshot.Rehydrate(
                p.Contenido,
                Enum.Parse<TipoLiberacion>(p.TipoLiberacion),
                p.SegundosLiberacion))
            .ToList()
            .AsReadOnly();

        return EtapaBusquedaTesoroSnapshot.Rehydrate(
            new EtapaId(e.EtapaId),
            e.Orden,
            e.Descripcion ?? string.Empty,
            e.CodigoQRSolucion ?? string.Empty,
            pistas);
    }
}
