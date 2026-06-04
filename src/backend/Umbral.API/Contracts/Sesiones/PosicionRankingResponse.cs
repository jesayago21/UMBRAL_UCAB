namespace Umbral.API.Contracts.Sesiones;

public sealed record PosicionRankingResponse(
    int Posicion,
    Guid ParticipanteId,
    string NombreParticipante,
    int PuntajeTotal);
