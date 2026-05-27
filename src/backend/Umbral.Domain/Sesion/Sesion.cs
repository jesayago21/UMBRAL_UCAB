using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Aggregate Root del BC EjecucionSesion.
///
/// Máquina de estados (umbral-backend-spec.md §2.4):
///   Programada → EnPreparacion → Activa ⇄ Pausada → Finalizada
///   Cualquier estado no terminal → Cancelada
///
/// Factory methods:
///   CrearBusquedaTesoro(snapshot, operadorId) → emite SesionCreada
///   CrearTrivia(preguntas, operadorId)         → emite SesionCreada (iter futura)
/// </summary>
public sealed class Sesion : AggregateRoot
{
    public SesionId SesionId { get; private set; } = default!;
    public TipoSesion TipoSesion { get; private set; }
    public UsuarioId OperadorId { get; private set; } = default!;
    public EstadoSesion Estado { get; private set; }
    public DateTime IniciadaEn { get; private set; }
    public DateTime? FinalizadaEn { get; private set; }

    public ContextoBusquedaTesoro? ContextoBT { get; private set; }

    private readonly List<EquipoSesion> _equipos = [];
    private readonly List<EventoSesion> _historialEventos = [];

    public IReadOnlyList<EquipoSesion> Equipos => _equipos.AsReadOnly();
    public IReadOnlyList<EventoSesion> HistorialEventos => _historialEventos.AsReadOnly();

    private Sesion() { }

    // ── Factory methods ─────────────────────────────────────────────────────

    public static Sesion CrearBusquedaTesoro(
        MisionSnapshot snapshot,
        UsuarioId operadorId)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(operadorId);

        var sesion = new Sesion
        {
            SesionId   = SesionId.Nuevo(),
            TipoSesion = TipoSesion.BusquedaTesoro,
            OperadorId = operadorId,
            Estado     = EstadoSesion.Programada,
            ContextoBT = ContextoBusquedaTesoro.Crear(snapshot)
        };
        sesion.RaiseDomainEvent(
            new SesionCreada(sesion.SesionId, TipoSesion.BusquedaTesoro, operadorId));
        return sesion;
    }

    // ── Comportamiento (máquina de estados) ─────────────────────────────────

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
        RegistrarEvento("SesionAbiertaParaRegistro", string.Empty);
    }

    /// <summary>
    /// Registra un equipo en la sesión (HU-13).
    /// RB-13-01: nombre único por sesión (insensible a mayúsculas).
    /// RB-13-02: no permitido en estados terminales (Finalizada, Cancelada).
    /// RB-13-03: genera <see cref="CodigoAcceso"/> único por equipo.
    /// </summary>
    public EquipoSesion RegistrarEquipo(string nombre)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException(
                "No se pueden registrar equipos en una sesión cerrada.");

        var nombreEquipo = NombreEquipo.Crear(nombre);

        if (_equipos.Any(e =>
                string.Equals(e.Nombre.Valor, nombreEquipo.Valor, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException(
                $"Ya existe un equipo con el nombre '{nombreEquipo.Valor}' en esta sesión.");

        var equipo = EquipoSesion.Crear(SesionId, nombreEquipo.Valor);
        _equipos.Add(equipo);
        RegistrarEvento("EquipoRegistrado", nombreEquipo.Valor);
        return equipo;
    }

    /// <summary>
    /// Inicia la sesión: EnPreparacion → Activa.
    /// R1: requiere al menos un equipo registrado.
    /// R2: solo válido desde EnPreparacion.
    /// </summary>
    public void Iniciar()
    {
        if (Estado != EstadoSesion.EnPreparacion)
            throw new DomainException(
                $"No se puede iniciar una sesión en estado '{Estado}'. " +
                "Solo es posible desde 'EnPreparacion'.");

        if (!_equipos.Any())
            throw new DomainException(
                "La sesión necesita al menos un equipo registrado.");

        Estado     = EstadoSesion.Activa;
        IniciadaEn = DateTime.UtcNow;
        RaiseDomainEvent(new SesionIniciada(SesionId, TipoSesion));
        RegistrarEvento("SesionIniciada", $"operador={OperadorId.Valor}");
    }

    /// <summary>Activa → Pausada (HU-15).</summary>
    public void Pausar()
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                $"No se puede pausar una sesión en estado '{Estado}'.");

        Estado = EstadoSesion.Pausada;
        RaiseDomainEvent(new Events.SesionPausada(SesionId));
        RegistrarEvento("SesionPausada", string.Empty);
    }

    /// <summary>Pausada → Activa (HU-15).</summary>
    public void Reanudar()
    {
        if (Estado != EstadoSesion.Pausada)
            throw new DomainException(
                $"No se puede reanudar una sesión en estado '{Estado}'.");

        Estado = EstadoSesion.Activa;
        RaiseDomainEvent(new Events.SesionReanudada(SesionId));
        RegistrarEvento("SesionReanudada", string.Empty);
    }

    /// <summary>Activa|Pausada → Finalizada.</summary>
    public void Finalizar()
    {
        if (Estado is not (EstadoSesion.Activa or EstadoSesion.Pausada))
            throw new DomainException(
                $"No se puede finalizar una sesión en estado '{Estado}'.");

        Estado       = EstadoSesion.Finalizada;
        FinalizadaEn = DateTime.UtcNow;
        RaiseDomainEvent(new Events.SesionFinalizada(SesionId));
        RegistrarEvento("SesionFinalizada", string.Empty);
    }

    public void Cancelar(string motivo)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException(
                $"No se puede cancelar una sesión en estado '{Estado}'.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("El motivo de cancelación no puede estar vacío.");

        Estado       = EstadoSesion.Cancelada;
        FinalizadaEn = DateTime.UtcNow;
    }

    /// <summary>
    /// Aplica una penalización a un equipo (HU-16).
    /// RB-16-01: solo en estado Activa.
    /// RB-16-02: el equipo debe pertenecer a la sesión.
    /// RB-16-03: emite PenalizacionAplicada; el puntaje no baja de cero.
    /// </summary>
    public void AplicarPenalizacion(EquipoId equipoId, Penalizacion penalizacion)
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                "Solo se pueden aplicar penalizaciones en sesiones activas.");

        var equipo = ObtenerEquipo(equipoId);
        equipo.AplicarPenalizacion(penalizacion);

        RaiseDomainEvent(new Events.PenalizacionAplicada(
            SesionId, equipoId,
            penalizacion.Puntos, penalizacion.Motivo,
            penalizacion.OperadorId));

        RegistrarEvento("PenalizacionAplicada",
            $"equipo={equipoId.Valor};puntos={penalizacion.Puntos};motivo={penalizacion.Motivo}");
    }

    public bool EstaActiva() => Estado == EstadoSesion.Activa;

    private EquipoSesion ObtenerEquipo(EquipoId equipoId) =>
        _equipos.FirstOrDefault(e => e.EquipoId == equipoId)
        ?? throw new DomainException(
            $"El equipo '{equipoId.Valor}' no pertenece a esta sesión.");

    private void RegistrarEvento(string tipo, string payload) =>
        _historialEventos.Add(EventoSesion.Crear(SesionId, tipo, payload));

    protected override bool IdEquals(Entity other) =>
        other is Sesion s && s.SesionId == SesionId;

    protected override int GetIdHashCode() => SesionId.GetHashCode();
}
