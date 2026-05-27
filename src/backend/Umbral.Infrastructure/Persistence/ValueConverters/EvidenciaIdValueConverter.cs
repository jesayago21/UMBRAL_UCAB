using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class EvidenciaIdValueConverter : ValueConverter<EvidenciaId, Guid>
{
    public EvidenciaIdValueConverter()
        : base(id => id.Valor, value => new EvidenciaId(value))
    {
    }
}
