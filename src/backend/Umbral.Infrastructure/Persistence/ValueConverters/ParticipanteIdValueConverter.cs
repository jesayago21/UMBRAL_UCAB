using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.Sesion;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class ParticipanteIdValueConverter : ValueConverter<ParticipanteId, Guid>
{
    public ParticipanteIdValueConverter()
        : base(id => id.Valor, value => new ParticipanteId(value))
    {
    }
}
