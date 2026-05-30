using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Infrastructure.Persistence.Serialization;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

internal sealed class OpcionesRespuestaJsonValueConverter
    : ValueConverter<List<OpcionRespuesta>, string>
{
    public OpcionesRespuestaJsonValueConverter()
        : base(
            opciones => OpcionesRespuestaPersistence.ToJson(opciones),
            json => OpcionesRespuestaPersistence.FromJson(json))
    {
    }
}
