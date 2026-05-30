using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class PreguntaIdValueConverter : ValueConverter<PreguntaId, Guid>
{
    public PreguntaIdValueConverter()
        : base(id => id.Valor, value => new PreguntaId(value))
    {
    }
}
