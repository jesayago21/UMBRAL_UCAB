using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Entrada de auditoría dentro del agregado Sesion (HU-22 / historial operativo).
/// </summary>
public sealed class EventoSesion : Entity
{
    public Guid EventoId { get; private set; }
    public SesionId SesionId { get; private set; } = default!;
    public string Tipo { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public DateTime OcurridoEn { get; private set; }

    private EventoSesion() { }

    internal static EventoSesion Crear(SesionId sesionId, string tipo, string payload)
    {
        ArgumentNullException.ThrowIfNull(sesionId);

        if (string.IsNullOrWhiteSpace(tipo))
            throw new DomainException("El tipo de evento de sesión no puede estar vacío.");

        return new EventoSesion
        {
            EventoId    = Guid.NewGuid(),
            SesionId    = sesionId,
            Tipo        = tipo.Trim(),
            Payload     = payload ?? string.Empty,
            OcurridoEn  = DateTime.UtcNow
        };
    }

    protected override bool IdEquals(Entity other) =>
        other is EventoSesion e && e.EventoId == EventoId;

    protected override int GetIdHashCode() => EventoId.GetHashCode();
}
