using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion.Events;

/// <summary>
/// Emitido al registrar cualquier envío de evidencia, válido o no (HU-18, RF-11).
/// </summary>
public sealed record EvidenciaRegistrada(
    SesionId             SesionId,
    ParticipanteId             ParticipanteId,
    EtapaId              EtapaId,
    ResultadoValidacion  Resultado,
    string               CodigoQR) : IDomainEvent
{
    public Guid     EventId     { get; } = Guid.NewGuid();
    public DateTime OcurridoEn { get; } = DateTime.UtcNow;
}
