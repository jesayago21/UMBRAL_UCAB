using FluentAssertions;
using Umbral.Infrastructure.Persistence.ValueConverters;
using Xunit;

namespace Umbral.Infrastructure.Tests.Persistence;

public sealed class CategoriaIdsJsonValueConverterTests
{
    private readonly CategoriaIdsJsonValueConverter _converter = new();

    [Fact]
    public void Convert_RoundTrip_PreservaLista()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var json = _converter.ConvertToProvider(ids)!;
        var roundTrip = _converter.ConvertFromProvider(json);

        roundTrip.Should().BeEquivalentTo(ids);
    }

    [Fact]
    public void Comparer_DetectaCambiosEnLista()
    {
        var a = new List<Guid> { Guid.NewGuid() };
        var b = new List<Guid> { Guid.NewGuid() };

        CategoriaIdsJsonValueConverter.Comparer.Equals(a, b).Should().BeFalse();
        CategoriaIdsJsonValueConverter.Comparer.Snapshot(a)
            .Should().NotBeSameAs(a);
    }
}
