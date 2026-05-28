namespace Umbral.API.Contracts.Sesiones;

public sealed record AplicarPenalizacionRequest(
    Guid EquipoId,
    int Puntos,
    string Motivo);
