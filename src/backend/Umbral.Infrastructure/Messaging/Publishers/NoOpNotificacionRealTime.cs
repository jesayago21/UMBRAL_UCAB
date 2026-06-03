using Umbral.Domain.Ports;

namespace Umbral.Infrastructure.Messaging.Publishers;

/// <summary>
/// Puerto E2: INotificacionRealTime se implementará con SignalR en Entrega 2.
/// Este NoOp permite que el contenedor DI resuelva el puerto sin fallos en E1.
/// </summary>
public sealed class NoOpNotificacionRealTime : INotificacionRealTime
{
    public Task NotificarCambioEstadoSesionAsync(
        string sesionId,
        string nuevoEstado,
        CancellationToken ct = default)
        => Task.CompletedTask;
}
