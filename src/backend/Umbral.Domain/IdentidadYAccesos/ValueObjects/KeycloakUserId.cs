using Umbral.Domain.Shared;

namespace Umbral.Domain.IdentidadYAccesos.ValueObjects;

public sealed class KeycloakUserId : ValueObject
{
    public Guid Value { get; }

    private KeycloakUserId(Guid value) => Value = value;

    public static KeycloakUserId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("KeycloakUserId no puede ser vacío.");

        return new KeycloakUserId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
