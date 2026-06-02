using System.Text.RegularExpressions;
using Umbral.Domain.Shared;

namespace Umbral.Domain.IdentidadYAccesos.ValueObjects;

public sealed partial class EmailAddress : ValueObject
{
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DomainException("El email no puede estar vacío.");

        var normalized = raw.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
            throw new DomainException("El formato del email no es válido.");

        return new EmailAddress(normalized);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
