using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.Shared;

namespace Umbral.Domain.IdentidadYAccesos.Events;

public sealed record UsuarioEstadoCambiado(
    UsuarioAdministrableId UsuarioId,
    EstadoUsuario NuevoEstado) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
