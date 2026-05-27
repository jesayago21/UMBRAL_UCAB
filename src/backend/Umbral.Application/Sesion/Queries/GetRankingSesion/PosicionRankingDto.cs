namespace Umbral.Application.Sesion.Queries.GetRankingSesion;

public sealed record PosicionRankingDto(
    int Posicion,
    Guid EquipoId,
    string NombreEquipo,
    int PuntajeTotal);
