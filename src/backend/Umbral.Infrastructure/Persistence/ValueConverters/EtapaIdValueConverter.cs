using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class EtapaIdValueConverter : ValueConverter<EtapaId, Guid>
{
    public EtapaIdValueConverter()
        : base(id => id.Valor, value => new EtapaId(value))
    {
    }
}
