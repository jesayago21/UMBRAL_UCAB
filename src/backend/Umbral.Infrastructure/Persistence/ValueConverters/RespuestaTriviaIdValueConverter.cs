using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class RespuestaTriviaIdValueConverter : ValueConverter<RespuestaTriviaId, Guid>
{
    public RespuestaTriviaIdValueConverter()
        : base(id => id.Valor, value => new RespuestaTriviaId(value))
    {
    }
}
