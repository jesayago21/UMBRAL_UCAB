namespace Umbral.Domain.Sesion;

/// <summary>Resultado de un tick del ciclo automático de trivia (HU-33 / HU-38).</summary>
public enum ResultadoCicloTrivia
{
    Ninguna,
    EntroEnTransicion,
    SiguientePreguntaLanzada,
    /// <summary>Se agotaron las preguntas de la etapa Trivia (HU-38).</summary>
    SecuenciaCompletada,
    /// <summary>Tras agotar trivia se avanzó a la siguiente etapa de la misión.</summary>
    EtapaAvanzada
}
