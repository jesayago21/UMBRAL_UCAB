using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class KeycloakUserIdValueConverter : ValueConverter<KeycloakUserId, Guid>
{
    public KeycloakUserIdValueConverter()
        : base(id => id.Value, g => KeycloakUserId.From(g))
    {
    }
}
