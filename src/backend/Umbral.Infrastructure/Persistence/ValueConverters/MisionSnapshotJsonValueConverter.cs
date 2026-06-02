using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Infrastructure.Persistence.Serialization;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

internal sealed class MisionSnapshotJsonValueConverter
    : ValueConverter<MisionSnapshot, string>
{
    public MisionSnapshotJsonValueConverter()
        : base(
            snapshot => MisionSnapshotPersistence.ToJson(snapshot),
            json => MisionSnapshotPersistence.FromJson(json))
    {
    }
}
