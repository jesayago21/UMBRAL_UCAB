namespace Umbral.API.Contracts.Sesiones;

public sealed record PosicionRankingResponse(
    int Posicion,
    Guid EquipoId,
    string NombreEquipo,
    int PuntajeTotal);
