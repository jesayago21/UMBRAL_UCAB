using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Sesion.Validacion;
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
///   CrearTrivia(preguntas, operadorId)         → emite SesionCreada
/// </summary>
public sealed class Sesion : AggregateRoot
{
    public SesionId SesionId { get; private set; } = default!;
    public TipoSesion TipoSesion { get; private set; }
    public UsuarioId OperadorId { get; private set; } = default!;
    public EstadoSesion Estado { get; private set; }
    public CodigoAcceso CodigoAcceso { get; private set; } = default!;
    public DateTime IniciadaEn { get; private set; }
    public DateTime? FinalizadaEn { get; private set; }

    public ContextoBusquedaTesoro? ContextoBT { get; private set; }
    public ContextoTrivia? ContextoTrivia { get; private set; }

    private readonly List<EquipoSesion> _equipos = [];
    private readonly List<EventoSesion> _historialEventos = [];
    private readonly List<Evidencia> _evidencias = [];

    public IReadOnlyList<EquipoSesion> Equipos => _equipos.AsReadOnly();
    public IReadOnlyList<EventoSesion> HistorialEventos => _historialEventos.AsReadOnly();
    public IReadOnlyList<Evidencia> Evidencias => _evidencias.AsReadOnly();

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
            SesionId     = SesionId.Nuevo(),
            TipoSesion   = TipoSesion.BusquedaTesoro,
            OperadorId   = operadorId,
            Estado       = EstadoSesion.Programada,
            CodigoAcceso = CodigoAcceso.Generar(),
            ContextoBT   = ContextoBusquedaTesoro.Crear(snapshot),
        };
        sesion.RaiseDomainEvent(
            new SesionCreada(sesion.SesionId, TipoSesion.BusquedaTesoro, operadorId));
        return sesion;
    }

    public static Sesion CrearTrivia(
        IReadOnlyList<PreguntaId> preguntasOrdenadas,
        UsuarioId operadorId)
    {
        ArgumentNullException.ThrowIfNull(preguntasOrdenadas);
        ArgumentNullException.ThrowIfNull(operadorId);

        var sesion = new Sesion
        {
            SesionId       = SesionId.Nuevo(),
            TipoSesion     = TipoSesion.Trivia,
            OperadorId     = operadorId,
            Estado         = EstadoSesion.Programada,
            CodigoAcceso   = CodigoAcceso.Generar(),
            ContextoTrivia = ContextoTrivia.Crear(preguntasOrdenadas)
        };
        sesion.RaiseDomainEvent(
            new SesionCreada(sesion.SesionId, TipoSesion.Trivia, operadorId));
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
    /// Un jugador autenticado se une a la sesión con el código de acceso de la sesión (RB-02).
    /// Solo en Programada o EnPreparacion; abre inscripción si aún está programada.
    /// </summary>
    public EquipoSesion UnirseEquipo(UsuarioId jugadorId, string nombre, string codigoAccesoIngresado)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException(
                "No se pueden unir equipos a una sesión cerrada.");

        if (Estado is EstadoSesion.Activa or EstadoSesion.Pausada)
            throw new DomainException(
                "La sesión ya está en juego. Solo puedes unirte antes de que el operador la inicie.");

        if (!CodigoAcceso.CoincideCon(codigoAccesoIngresado))
            throw new DomainException("El código de acceso de la sesión no es válido.");

        if (Estado == EstadoSesion.Programada)
            AbrirParaRegistro();

        if (_equipos.Any(e => e.JugadorId == jugadorId))
            throw new DomainException("Ya estás inscrito en esta sesión.");

        var nombreEquipo = NombreEquipo.Crear(nombre);

        if (_equipos.Any(e =>
                string.Equals(e.Nombre.Valor, nombreEquipo.Valor, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException(
                $"Ya existe un equipo con el nombre '{nombreEquipo.Valor}' en esta sesión.");

        var equipo = EquipoSesion.Crear(SesionId, jugadorId, nombreEquipo.Valor);
        _equipos.Add(equipo);
        RegistrarEvento("EquipoUnido", nombreEquipo.Valor);
        return equipo;
    }

    /// <summary>
    /// Inicia la sesión: EnPreparacion → Activa (HU-14).
    /// RB-18: requiere al menos un equipo registrado.
    /// Criterios iter-03: RB-14-01…05.
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

    /// <summary>Activa|Pausada → Finalizada (HU-23).</summary>
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

    /// <summary>
    /// Cancela la sesión desde cualquier estado no terminal (HU-23).
    /// Emite <see cref="Events.SesionCancelada"/> y registra motivo en historial.
    /// </summary>
    public void Cancelar(string motivo)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException(
                $"No se puede cancelar una sesión en estado '{Estado}'.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("El motivo de cancelación no puede estar vacío.");

        var motivoLimpio = motivo.Trim();
        Estado       = EstadoSesion.Cancelada;
        FinalizadaEn = DateTime.UtcNow;
        RaiseDomainEvent(new Events.SesionCancelada(SesionId, motivoLimpio));
        RegistrarEvento("SesionCancelada", motivoLimpio);
    }

    /// <summary>
    /// Ranking final ordenado por puntaje (HU-23). Solo en sesiones cerradas.
    /// </summary>
    public IReadOnlyList<PosicionRanking> ObtenerRankingFinal()
    {
        if (Estado is not (EstadoSesion.Finalizada or EstadoSesion.Cancelada))
            throw new DomainException(
                "Solo se puede obtener el ranking de sesiones finalizadas o canceladas.");

        return RankingService.Calcular(Equipos);
    }

    /// <summary>
    /// Aplica una penalización a un equipo (HU-16).
    /// Criterios iter-04: RB-16-01…05. Globales: RB-20 (motivo), RB-24 (piso 0), RB-25 (OperadorId).
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

    /// <summary>
    /// Registra evidencia QR enviada por un equipo (HU-18).
    /// RB-06, RB-19, RB-22. HU-19/RB-04: ganador único. HU-20/RB-05: transición de etapa.
    /// </summary>
    public Evidencia RegistrarEvidencia(EquipoId equipoId, string codigoQR)
    {
        if (TipoSesion != TipoSesion.BusquedaTesoro || ContextoBT is null)
            throw new DomainException(
                "Solo las sesiones de Búsqueda del Tesoro aceptan evidencias QR.");

        var equipo = ObtenerEquipo(equipoId);
        var qr     = CodigoQR.Crear(codigoQR);
        var etapa  = ContextoBT.ObtenerEtapaActual();

        var resultado = ValidacionEvidenciaService.Validar(this, qr);

        if (resultado == ResultadoValidacion.Valida &&
            _evidencias.Any(e =>
                e.EquipoId == equipoId &&
                e.EtapaId == etapa.EtapaId &&
                e.Resultado == ResultadoValidacion.Valida))
        {
            resultado = ResultadoValidacion.Invalida;
        }

        if (resultado == ResultadoValidacion.Valida &&
            ContextoBT.YaHayGanadorEnEtapaActual())
        {
            resultado = ResultadoValidacion.Invalida;
        }

        var evidencia = Evidencia.Registrar(
            SesionId, equipo.EquipoId, etapa.EtapaId, qr, resultado);
        _evidencias.Add(evidencia);

        RaiseDomainEvent(new EvidenciaRegistrada(
            SesionId, equipo.EquipoId, etapa.EtapaId, resultado, qr.Valor));

        RegistrarEvento("EvidenciaRegistrada",
            $"equipo={equipo.EquipoId.Valor};etapa={etapa.EtapaId.Valor};resultado={resultado};qr={qr.Valor}");

        if (resultado == ResultadoValidacion.Valida)
            ProcesarEvidenciaGanadora(equipo, etapa);

        return evidencia;
    }

    private void ProcesarEvidenciaGanadora(EquipoSesion equipo, EtapaSnapshot etapa)
    {
        var puntos = CalculoPuntajeBusquedaService.Calcular(esGanador: true);
        equipo.SumarPuntaje(puntos.Valor);

        ContextoBT!.RegistrarGanadorEtapa(equipo.EquipoId);

        RaiseDomainEvent(new EvidenciaValidada(
            SesionId, equipo.EquipoId, etapa.EtapaId, puntos));

        var indexCompletada = ContextoBT.EtapaActualIndex;
        var esUltima        = ContextoBT.EsUltimaEtapa();

        RaiseDomainEvent(new EtapaCompletada(
            SesionId, indexCompletada, equipo.EquipoId));

        RegistrarEvento("EtapaCompletada",
            $"etapaIndex={indexCompletada};ganador={equipo.EquipoId.Valor}");

        if (esUltima)
            Finalizar();
        else
            ContextoBT.AvanzarEtapa();
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
