using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class CategoriaIdsJsonValueConverter : ValueConverter<List<Guid>, string>
{
    private static readonly JsonSerializerOptions Options = new();

    public CategoriaIdsJsonValueConverter()
        : base(
            v => JsonSerializer.Serialize(v, Options),
            v => JsonSerializer.Deserialize<List<Guid>>(v, Options) ?? new List<Guid>())
    {
    }
}
