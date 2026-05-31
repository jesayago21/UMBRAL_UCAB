using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Infrastructure.Persistence.Serialization;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

internal sealed class PreguntasOrdenadasJsonValueConverter
    : ValueConverter<IReadOnlyList<PreguntaId>, string>
{
    public PreguntasOrdenadasJsonValueConverter()
        : base(
            list => PreguntasOrdenadasPersistence.ToJson(list),
            json => PreguntasOrdenadasPersistence.FromJson(json))
    {
    }
}
