using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Umbral.Infrastructure.RealTime.Hubs;

/// <summary>
/// Hub de eventos comunes de sesión + Búsqueda del tesoro. Sin lógica de negocio.
/// </summary>
[Authorize]
public sealed class SesionHub : Hub<ISesionHubClient>
{
    private readonly ILogger<SesionHub> _logger;

    public SesionHub(ILogger<SesionHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("SesionHub conectado: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "SesionHub desconectado: {ConnectionId} | {Error}",
            Context.ConnectionId,
            exception?.Message ?? "Normal");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Participante: grupo de sesión + grupo privado del equipo.</summary>
    public async Task UnirseASesion(Guid sesionId, Guid participanteId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"sesion-{sesionId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"equipo-{participanteId}");
        _logger.LogInformation(
            "Participante {ParticipanteId} unido a sesion-{SesionId}",
            participanteId,
            sesionId);
    }

    /// <summary>Operador/Admin: grupo de sesión + grupo de operadores.</summary>
    public async Task UnirseComoOperador(Guid sesionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"sesion-{sesionId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"operador-{sesionId}");
        _logger.LogInformation("Operador unido a sesion-{SesionId}", sesionId);
    }
}
