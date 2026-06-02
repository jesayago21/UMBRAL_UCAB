using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

/// <summary>
/// Emitido cuando un participante gana la etapa con evidencia válida (HU-19, RB-04).
/// </summary>
public sealed record EvidenciaValidada(
    SesionId  SesionId,
    ParticipanteId  ParticipanteGanadorId,
    EtapaId   EtapaId,
    Puntaje   PuntosOtorgados) : IDomainEvent
{
    public Guid     EventId     { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
