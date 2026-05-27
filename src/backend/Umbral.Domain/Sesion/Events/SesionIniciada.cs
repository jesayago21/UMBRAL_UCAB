using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

public sealed record SesionIniciada(
    SesionId SesionId,
    TipoSesion TipoSesion) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
