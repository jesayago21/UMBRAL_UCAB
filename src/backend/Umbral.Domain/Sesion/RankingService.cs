namespace Umbral.Domain.Sesion;

/// <summary>
/// Domain Service — ranking por puntaje descendente (HU-23, RB-08).
/// </summary>
public static class RankingService
{
    public static IReadOnlyList<PosicionRanking> Calcular(IReadOnlyList<EquipoSesion> equipos)
    {
        ArgumentNullException.ThrowIfNull(equipos);

        return equipos
            .OrderByDescending(e => e.PuntajeTotal.Valor)
            .ThenBy(e => e.Nombre.Valor, StringComparer.OrdinalIgnoreCase)
            .Select((e, index) => new PosicionRanking(
                index + 1,
                e.EquipoId,
                e.Nombre.Valor,
                e.PuntajeTotal.Valor))
            .ToList();
    }
}
