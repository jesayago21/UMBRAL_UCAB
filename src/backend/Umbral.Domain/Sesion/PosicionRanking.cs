namespace Umbral.Domain.Sesion;

/// <summary>
/// Posición en el ranking de una sesión (HU-23 / HU-39, RB-08).
/// </summary>
public sealed record PosicionRanking(
    int Posicion,
    ParticipanteId ParticipanteId,
    string NombreParticipante,
    int PuntajeTotal,
    long TiempoAcumuladoMs);
