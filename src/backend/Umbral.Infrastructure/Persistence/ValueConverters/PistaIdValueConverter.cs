using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class PistaIdValueConverter : ValueConverter<PistaId, Guid>
{
    public PistaIdValueConverter()
        : base(id => id.Valor, value => new PistaId(value))
    {
    }
}
