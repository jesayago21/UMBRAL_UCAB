namespace Umbral.Domain.Sesion;

/// <summary>
/// Domain Service — puntaje Trivia (TriviaStrategy: base 100 + bonus velocidad hasta 50).
/// </summary>
public static class CalculoPuntajeTriviaService
{
    private const int PuntajeBase = 100;
    private const int BonusVelocidadMax = 50;

    public static Puntaje Calcular(bool esCorrecta, bool fueraDeTiempo, long tiempoRespuestaMs, long timerTotalMs)
    {
        if (!esCorrecta || fueraDeTiempo)
            return Puntaje.Zero();

        var bonus = CalcularBonusVelocidad(tiempoRespuestaMs, timerTotalMs);
        return Puntaje.Crear(PuntajeBase + bonus);
    }

    private static int CalcularBonusVelocidad(long tiempoMs, long timerTotalMs)
    {
        if (timerTotalMs <= 0)
            return 0;

        var fraccion = 1.0 - ((double)tiempoMs / timerTotalMs);
        return (int)(BonusVelocidadMax * Math.Max(0, fraccion));
    }
}
