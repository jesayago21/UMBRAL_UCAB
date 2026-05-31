using System.Text.Json;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

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
        public Guid EtapaId { get; init; }
        public int Orden { get; init; }
        public string Descripcion { get; init; } = string.Empty;
        public string CodigoQRSolucion { get; init; } = string.Empty;
        public List<PistaSnapshotDto>? Pistas { get; init; }
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
            Etapas   = snapshot.Etapas
                .Select(e => new EtapaSnapshotDto
                {
                    EtapaId          = e.EtapaId.Valor,
                    Orden            = e.Orden,
                    Descripcion      = e.Descripcion,
                    CodigoQRSolucion = e.CodigoQRSolucion,
                    Pistas           = e.Pistas
                        .Select(p => new PistaSnapshotDto
                        {
                            Contenido          = p.Contenido,
                            TipoLiberacion     = p.TipoLiberacion.ToString(),
                            SegundosLiberacion = p.SegundosLiberacion
                        })
                        .ToList()
                })
                .ToList()
        };

        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public static MisionSnapshot FromJson(string json)
    {
        var dto = JsonSerializer.Deserialize<MisionSnapshotDto>(json, JsonOptions)
                  ?? throw new InvalidOperationException("JSON de MisionSnapshot inválido.");

        var etapas = dto.Etapas
            .Select(e =>
            {
                var pistas = (e.Pistas ?? [])
                    .Select(p => PistaSnapshot.Rehydrate(
                        p.Contenido,
                        Enum.Parse<TipoLiberacion>(p.TipoLiberacion),
                        p.SegundosLiberacion))
                    .ToList()
                    .AsReadOnly();

                return EtapaSnapshot.Rehydrate(
                    new EtapaId(e.EtapaId),
                    e.Orden,
                    e.Descripcion,
                    e.CodigoQRSolucion,
                    pistas);
            })
            .ToList()
            .AsReadOnly();

        return MisionSnapshot.Rehydrate(
            new MisionId(dto.MisionId),
            dto.Nombre,
            etapas);
    }
}
