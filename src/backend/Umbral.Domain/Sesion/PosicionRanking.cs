namespace Umbral.Domain.Sesion;

/// <summary>
/// Posición en el ranking final de una sesión cerrada (HU-23, RB-08).
/// </summary>
public sealed record PosicionRanking(
    int      Posicion,
    EquipoId EquipoId,
    string   NombreEquipo,
    int      PuntajeTotal);
