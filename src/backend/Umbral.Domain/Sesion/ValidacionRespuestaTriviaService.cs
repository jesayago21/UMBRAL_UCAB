namespace Umbral.Domain.Sesion;

/// <summary>HU-34 / RB-12 — determina si la respuesta llegó tras el cierre del timer.</summary>
public static class ValidacionRespuestaTriviaService
{
    public static bool EsFueraDeTiempo(DateTime timestampUtc, DateTime? timerCerradoEnUtc)
    {
        if (timerCerradoEnUtc is null)
            return true;

        return timestampUtc >= timerCerradoEnUtc.Value;
    }
}
