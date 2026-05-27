namespace Umbral.Domain.Sesion;

/// <summary>
/// Ciclo de vida de una sesión.
/// Transiciones válidas:
///   Programada    → EnPreparacion  (AbrirParaRegistro)
///   EnPreparacion → Activa         (Iniciar)
///   Activa        → Pausada        (Pausar)
///   Pausada       → Activa         (Reanudar)
///   Activa|Pausada → Finalizada    (Finalizar)
///   *             → Cancelada      (Cancelar, salvo Finalizada/Cancelada)
/// </summary>
public enum EstadoSesion
{
    Programada    = 1,
    EnPreparacion = 2,
    Activa        = 3,
    Pausada       = 4,
    Finalizada    = 5,
    Cancelada     = 6
}
