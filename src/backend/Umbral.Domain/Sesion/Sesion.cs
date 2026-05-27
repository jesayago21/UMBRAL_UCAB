using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Raíz de agregado del contexto de Ejecución de Sesión.
///
/// Máquina de estados (según umbral-backend-spec.md):
///   Programada    ──AbrirParaRegistro()──► EnPreparacion
///   EnPreparacion ──Iniciar()──────────►  Activa
///   Activa        ──Pausar()──────────►   Pausada    (iter futura)
///   Pausada       ──Reanudar()────────►   Activa     (iter futura)
///   Activa|Pausada──Finalizar()────────►  Finalizada (iter futura)
///   *             ──Cancelar(motivo)──►   Cancelada  (iter futura)
///
/// Reglas de Iter 1 (IniciarSesion):
///   R1 — No se puede iniciar sin al menos un equipo registrado.
///   R2 — Solo se puede iniciar desde EnPreparacion.
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
        Estado     = EstadoSesion.Programada;
    }

    // ── Factory ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea una nueva sesión en estado Programada.
    /// Simplificado para Fase 1 (sin MisionSnapshot ni OperadorId,
    /// que se incorporan cuando se implemente Application + EF Core).
    /// </summary>
    public static Sesion Crear(SesionId id, TipoSesion tipoSesion)
    {
        ArgumentNullException.ThrowIfNull(id);
        return new Sesion(id, tipoSesion);
    }

    // ── Comportamiento ───────────────────────────────────────────────────────

    /// <summary>
    /// Abre la sesión para registro de equipos: Programada → EnPreparacion.
    /// </summary>
    public void AbrirParaRegistro()
    {
        if (Estado != EstadoSesion.Programada)
            throw new DomainException(
                $"No se puede abrir para registro una sesión en estado '{Estado}'. " +
                "Solo es posible desde 'Programada'.");

        Estado = EstadoSesion.EnPreparacion;
    }

    /// <summary>
    /// Registra un equipo participante.
    /// Iter 1: versión mínima (solo agrega, sin nombre único ni rechazo terminal).
    /// Iter 2 completará las reglas de negocio completas.
    /// </summary>
    public void RegistrarEquipo(EquipoSesion equipo)
    {
        ArgumentNullException.ThrowIfNull(equipo);
        _equipos.Add(equipo);
    }

    /// <summary>
    /// Inicia la sesión: EnPreparacion → Activa.
    /// R1: requiere al menos un equipo registrado.
    /// R2: solo válido desde EnPreparacion.
    /// </summary>
    public void Iniciar()
    {
        // R2 — transición válida de estado
        if (Estado != EstadoSesion.EnPreparacion)
            throw new DomainException(
                $"No se puede iniciar una sesión en estado '{Estado}'. " +
                "Solo es posible desde 'EnPreparacion'.");

        // R1 — debe haber al menos un equipo
        if (_equipos.Count == 0)
            throw new DomainException(
                "La sesión necesita al menos un equipo registrado.");

        Estado     = EstadoSesion.Activa;
        IniciadaEn = DateTime.UtcNow;

        AddDomainEvent(new SesionIniciada(Id, TipoSesion, IniciadaEn.Value));
    }
}
