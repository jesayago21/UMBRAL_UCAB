using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class EquipoIdValueConverter : ValueConverter<EquipoId, Guid>
{
    public EquipoIdValueConverter()
        : base(id => id.Valor, value => new EquipoId(value))
    {
    }
}
