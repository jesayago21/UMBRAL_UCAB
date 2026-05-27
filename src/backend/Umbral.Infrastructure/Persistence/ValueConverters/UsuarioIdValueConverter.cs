using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class UsuarioIdValueConverter : ValueConverter<UsuarioId, Guid>
{
    public UsuarioIdValueConverter()
        : base(id => id.Valor, value => new UsuarioId(value))
    {
    }
}
