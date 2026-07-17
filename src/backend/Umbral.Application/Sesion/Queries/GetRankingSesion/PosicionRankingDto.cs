namespace Umbral.Application.Sesion.Queries.GetRankingSesion;

public sealed record PosicionRankingDto(
    int Posicion,
    Guid ParticipanteId,
    string NombreParticipante,
    int PuntajeTotal,
    long TiempoAcumuladoMs = 0);
