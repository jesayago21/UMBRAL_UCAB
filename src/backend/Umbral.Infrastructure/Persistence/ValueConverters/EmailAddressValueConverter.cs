using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;

namespace Umbral.Infrastructure.Persistence.ValueConverters;

public sealed class EmailAddressValueConverter : ValueConverter<EmailAddress, string>
{
    public EmailAddressValueConverter()
        : base(e => e.Value, s => EmailAddress.Create(s))
    {
    }
}
