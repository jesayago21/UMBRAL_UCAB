using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;

namespace Umbral.Domain.IdentidadYAccesos.Events;

public sealed record UsuarioCreadoEnDominio(
    UsuarioAdministrableId UsuarioId,
    KeycloakUserId KeycloakUserId,
    EmailAddress Email) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
