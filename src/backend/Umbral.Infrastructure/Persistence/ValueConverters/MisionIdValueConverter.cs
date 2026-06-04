using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.CatalogoMision.Mision;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class MisionIdValueConverter : ValueConverter<MisionId, Guid>
{
    public MisionIdValueConverter()
        : base(id => id.Valor, value => new MisionId(value))
    {
    }
}
