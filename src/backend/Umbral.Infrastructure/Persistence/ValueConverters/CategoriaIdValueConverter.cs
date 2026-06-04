using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.CatalogoTrivia.Categoria;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class CategoriaIdValueConverter : ValueConverter<CategoriaId, Guid>
{
    public CategoriaIdValueConverter()
        : base(id => id.Valor, value => new CategoriaId(value))
    {
    }
}
