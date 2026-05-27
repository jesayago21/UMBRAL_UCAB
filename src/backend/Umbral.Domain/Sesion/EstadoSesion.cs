namespace Umbral.Domain.Sesion;

/// <summary>
/// Ciclo de vida de una sesión.
/// Las transiciones válidas son:
///   Preparacion → Activa → Pausada → Activa
///                         Activa  → Finalizada
///                         Activa  → Cancelada
///   Preparacion → Cancelada
/// </summary>
public enum EstadoSesion
{
    Preparacion = 1,
    Activa      = 2,
    Pausada     = 3,
    Finalizada  = 4,
    Cancelada   = 5
}
