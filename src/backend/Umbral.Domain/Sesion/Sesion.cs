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
    public IReadOnlyList<EquipoSesion> Equipos => _equipos.AsReadOnly();

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
    }

    /// <summary>
    /// Registra un equipo: EnPreparacion o Programada permitidos.
    /// Iter 2 agrega: nombre único + rechazo en estado terminal.
    /// </summary>
    public EquipoSesion RegistrarEquipo(string nombre)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException(
                "No se pueden registrar equipos en una sesión cerrada.");

        if (_equipos.Any(e => e.Nombre.Valor == nombre))
            throw new DomainException(
                $"Ya existe un equipo con el nombre '{nombre}' en esta sesión.");

        var equipo = EquipoSesion.Crear(SesionId, nombre);
        _equipos.Add(equipo);
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
    }

    public void Pausar()
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                $"No se puede pausar una sesión en estado '{Estado}'.");

        Estado = EstadoSesion.Pausada;
    }

    public void Reanudar()
    {
        if (Estado != EstadoSesion.Pausada)
            throw new DomainException(
                $"No se puede reanudar una sesión en estado '{Estado}'.");

        Estado = EstadoSesion.Activa;
    }

    public void Finalizar()
    {
        if (Estado is not (EstadoSesion.Activa or EstadoSesion.Pausada))
            throw new DomainException(
                $"No se puede finalizar una sesión en estado '{Estado}'.");

        Estado       = EstadoSesion.Finalizada;
        FinalizadaEn = DateTime.UtcNow;
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

    public void AplicarPenalizacion(EquipoId equipoId, Penalizacion penalizacion)
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                "Solo se pueden aplicar penalizaciones en sesiones activas.");

        var equipo = ObtenerEquipo(equipoId);
        equipo.AplicarPenalizacion(penalizacion);
    }

    public bool EstaActiva() => Estado == EstadoSesion.Activa;

    private EquipoSesion ObtenerEquipo(EquipoId equipoId) =>
        _equipos.FirstOrDefault(e => e.EquipoId == equipoId)
        ?? throw new DomainException(
            $"El equipo '{equipoId.Valor}' no pertenece a esta sesión.");

    protected override bool IdEquals(Entity other) =>
        other is Sesion s && s.SesionId == SesionId;

    protected override int GetIdHashCode() => SesionId.GetHashCode();
}
