using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class CategoriaIdsJsonValueConverter : ValueConverter<List<Guid>, string>
{
    private static readonly JsonSerializerOptions Options = new();

    public CategoriaIdsJsonValueConverter()
        : base(
            v => JsonSerializer.Serialize(v ?? new List<Guid>(), Options),
            v => JsonSerializer.Deserialize<List<Guid>>(v, Options) ?? new List<Guid>())
    {
    }

    public static ValueComparer<List<Guid>> Comparer { get; } = new(
        (a, b) => SequenceEqual(a, b),
        v => GetHash(v),
        v => Snapshot(v));

    private static bool SequenceEqual(List<Guid>? a, List<Guid>? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.SequenceEqual(b);
    }

    private static int GetHash(List<Guid>? v)
    {
        if (v is null) return 0;
        var hash = 0;
        foreach (var id in v)
            hash = HashCode.Combine(hash, id);
        return hash;
    }

    private static List<Guid> Snapshot(List<Guid>? v) =>
        v is null ? new List<Guid>() : v.ToList();
}
