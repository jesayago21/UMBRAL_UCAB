using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

public sealed record SesionCreada(
    SesionId SesionId,
    TipoSesion TipoSesion,
    UsuarioId OperadorId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
