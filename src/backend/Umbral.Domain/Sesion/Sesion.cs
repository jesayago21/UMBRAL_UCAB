using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Raíz de agregado del contexto de Ejecución de Sesión.
///
/// Estados y transiciones:
///   Preparacion ──Iniciar()──► Activa
///   Activa      ──Pausar()──►  Pausada  (Iter futura)
///   Pausada     ──Reanudar()─► Activa   (Iter futura)
///   Activa      ──Finalizar()► Finalizada (Iter futura)
///   *           ──Cancelar()─► Cancelada  (Iter futura)
///
/// Reglas de Iter 1 (IniciarSesion):
///   R1 - No se puede iniciar una sesión sin equipos registrados.
///   R2 - Solo se puede iniciar desde el estado Preparacion.
/// </summary>
public sealed class Sesion : AggregateRoot<SesionId>
{
    private readonly List<EquipoSesion> _equipos = [];

    public TipoSesion TipoSesion { get; }
    public EstadoSesion Estado { get; private set; }
    public DateTime? IniciadaEn { get; private set; }

    public IReadOnlyList<EquipoSesion> Equipos => _equipos.AsReadOnly();

    private Sesion(SesionId id, TipoSesion tipoSesion) : base(id)
    {
        TipoSesion = tipoSesion;
        Estado     = EstadoSesion.Preparacion;
    }

    // ── Factory ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea una nueva sesión en estado Preparacion.
    /// El operador todavía no ha registrado equipos.
    /// </summary>
    public static Sesion Crear(SesionId id, TipoSesion tipoSesion)
    {
        ArgumentNullException.ThrowIfNull(id);
        return new Sesion(id, tipoSesion);
    }

    // ── Comportamiento ───────────────────────────────────────────────────────

    /// <summary>
    /// Registra un equipo participante.
    /// Iter 1: versión mínima (solo agrega).
    /// Iter 2 añadirá: nombre único por sesión, rechazo en estado terminal.
    /// </summary>
    public void RegistrarEquipo(EquipoSesion equipo)
    {
        ArgumentNullException.ThrowIfNull(equipo);
        _equipos.Add(equipo);
    }

    /// <summary>
    /// Inicia la sesión: Preparacion → Activa.
    /// R1: requiere al menos un equipo registrado.
    /// R2: solo válido desde Preparacion.
    /// </summary>
    public void Iniciar(DateTime ahora)
    {
        // R2 — transición válida de estado
        if (Estado != EstadoSesion.Preparacion)
            throw new DomainException(
                $"No se puede iniciar una sesión en estado '{Estado}'. " +
                "Solo es posible desde 'Preparacion'.");

        // R1 — debe haber al menos un equipo
        if (_equipos.Count == 0)
            throw new DomainException(
                "No se puede iniciar la sesión sin equipos registrados.");

        Estado     = EstadoSesion.Activa;
        IniciadaEn = ahora;

        AddDomainEvent(new SesionIniciada(Id, ahora));
    }
}
