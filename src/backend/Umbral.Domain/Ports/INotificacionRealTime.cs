namespace Umbral.Domain.Ports;

public interface INotificacionRealTime
{
    Task NotificarCambioEstadoSesionAsync(
        string sesionId,
        string nuevoEstado,
        CancellationToken ct = default);
}
