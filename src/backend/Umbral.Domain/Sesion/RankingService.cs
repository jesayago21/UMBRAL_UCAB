namespace Umbral.Domain.Sesion;

/// <summary>
/// Domain Service — ranking por puntaje descendente; desempate por menor tiempo
/// acumulado de respuestas trivia a tiempo (HU-23 / HU-39, RB-08 / RB-12).
/// </summary>
public static class RankingService
{
    public static IReadOnlyList<PosicionRanking> Calcular(
        IReadOnlyList<ParticipanteSesion> participantes,
        IReadOnlyList<RespuestaTrivia>? respuestasTrivia = null)
    {
        ArgumentNullException.ThrowIfNull(participantes);

        var tiempoPorParticipante = CalcularTiemposAcumulados(respuestasTrivia);

        return participantes
            .Select(e => (
                Participante: e,
                TiempoMs: tiempoPorParticipante.GetValueOrDefault(e.ParticipanteId, 0L)))
            .OrderByDescending(x => x.Participante.PuntajeTotal.Valor)
            .ThenBy(x => x.TiempoMs)
            .ThenBy(x => x.Participante.Nombre.Valor, StringComparer.OrdinalIgnoreCase)
            .Select((x, index) => new PosicionRanking(
                index + 1,
                x.Participante.ParticipanteId,
                x.Participante.Nombre.Valor,
                x.Participante.PuntajeTotal.Valor,
                x.TiempoMs))
            .ToList();
    }

    /// <summary>
    /// Suma solo tiempos de respuestas a tiempo (RB-12: fuera de tiempo no es tiempo útil).
    /// </summary>
    internal static Dictionary<ParticipanteId, long> CalcularTiemposAcumulados(
        IReadOnlyList<RespuestaTrivia>? respuestasTrivia)
    {
        if (respuestasTrivia is null || respuestasTrivia.Count == 0)
            return new Dictionary<ParticipanteId, long>();

        return respuestasTrivia
            .Where(r => !r.FueraDeTiempo)
            .GroupBy(r => r.ParticipanteId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(r => r.TiempoRespuestaMs));
    }
}
