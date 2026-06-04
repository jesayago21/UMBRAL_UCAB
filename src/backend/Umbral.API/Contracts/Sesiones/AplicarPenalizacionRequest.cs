namespace Umbral.API.Contracts.Sesiones;

public sealed record AplicarPenalizacionRequest(
    Guid ParticipanteId,
    int Puntos,
    string Motivo);
