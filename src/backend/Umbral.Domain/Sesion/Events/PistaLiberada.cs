using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

/// <summary>Domain event — pista liberada a un participante (PorTiempo, AlInicio/PorGanador o manual) (RB-07, RB-21).</summary>
public sealed record PistaLiberada(
    SesionId SesionId,
    ParticipanteId ParticipanteId,
    PistaId PistaId,
    int EtapaIndex,
    string Contenido) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
