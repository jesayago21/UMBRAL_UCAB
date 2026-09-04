using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.Sesion;
using Umbral.Infrastructure.Persistence.Serialization;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

internal sealed class PistasEntregadasJsonValueConverter
    : ValueConverter<IReadOnlyList<PistaEntregada>, string>
{
    private static readonly IReadOnlyList<PistaEntregada> Empty = Array.Empty<PistaEntregada>();

    public static readonly ValueComparer Comparer = new ValueComparer<IReadOnlyList<PistaEntregada>>(
        (a, b) => PistasEntregadasPersistence.ToJson(a ?? Empty)
                  == PistasEntregadasPersistence.ToJson(b ?? Empty),
        v => PistasEntregadasPersistence.ToJson(v ?? Empty).GetHashCode(),
        v => PistasEntregadasPersistence.FromJson(
            PistasEntregadasPersistence.ToJson(v ?? Empty)));

    public PistasEntregadasJsonValueConverter()
        : base(
            list => PistasEntregadasPersistence.ToJson(list ?? Empty),
            json => PistasEntregadasPersistence.FromJson(json))
    {
    }
}
