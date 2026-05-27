using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class SesionIdValueConverter : ValueConverter<SesionId, Guid>
{
    public SesionIdValueConverter()
        : base(id => id.Valor, value => new SesionId(value))
    {
    }
}
