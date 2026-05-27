using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

/// <summary>
/// Emitido cuando el operador aplica una penalización a un equipo (HU-16).
/// umbral-backend-spec.md §2.4
/// </summary>
public sealed record PenalizacionAplicada(
    SesionId  SesionId,
    EquipoId  EquipoId,
    int       Puntos,
    string    Motivo,
    UsuarioId OperadorId) : IDomainEvent
{
    public Guid     EventId     { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
