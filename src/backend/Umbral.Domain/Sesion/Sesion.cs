using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion.Events;
using Umbral.Domain.Sesion.Validacion;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Aggregate Root del BC EjecucionSesion — sesión de misión polimórfica.
/// </summary>
public sealed class Sesion : AggregateRoot
{
    public SesionId SesionId { get; private set; } = default!;
    public TipoSesion TipoSesion { get; private set; }
    public MisionId? MisionId { get; private set; }
    public UsuarioId OperadorId { get; private set; } = default!;
    public EstadoSesion Estado { get; private set; }
    public CodigoAcceso CodigoAcceso { get; private set; } = default!;
    public DateTime IniciadaEn { get; private set; }
    public DateTime? FinalizadaEn { get; private set; }

    public ContextoMision? ContextoMision { get; private set; }

    [Obsolete("Usar ContextoMision. Mantenido para migración de datos legacy.")]
    public ContextoBusquedaTesoro? ContextoBT { get; private set; }

    [Obsolete("Usar ContextoMision. Mantenido para migración de datos legacy.")]
    public ContextoTrivia? ContextoTrivia { get; private set; }

    private readonly List<ParticipanteSesion> _participantes = [];
    private readonly List<EventoSesion> _historialEventos = [];
    private readonly List<Evidencia> _evidencias = [];

    public IReadOnlyList<ParticipanteSesion> Participantes => _participantes.AsReadOnly();
    public IReadOnlyList<EventoSesion> HistorialEventos => _historialEventos.AsReadOnly();
    public IReadOnlyList<Evidencia> Evidencias => _evidencias.AsReadOnly();

    private Sesion() { }

    public static Sesion CrearDesdeMision(MisionSnapshot snapshot, UsuarioId operadorId)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(operadorId);

        var sesion = new Sesion
        {
            SesionId       = SesionId.Nuevo(),
            TipoSesion     = TipoSesion.Mision,
            MisionId       = snapshot.MisionId,
            OperadorId     = operadorId,
            Estado         = EstadoSesion.Programada,
            CodigoAcceso   = CodigoAcceso.Generar(),
            ContextoMision = ContextoMision.Crear(snapshot)
        };
        sesion.RaiseDomainEvent(
            new SesionCreada(sesion.SesionId, TipoSesion.Mision, operadorId));
        return sesion;
    }

    [Obsolete("Usar CrearDesdeMision")]
    public static Sesion CrearBusquedaTesoro(MisionSnapshot snapshot, UsuarioId operadorId) =>
        CrearDesdeMision(snapshot, operadorId);

    [Obsolete("Usar CrearDesdeMision con misión que incluya etapas trivia")]
    public static Sesion CrearTrivia(
        IReadOnlyList<PreguntaId> preguntasOrdenadas,
        UsuarioId operadorId,
        string categoriasTitulo)
    {
        ArgumentNullException.ThrowIfNull(preguntasOrdenadas);
        ArgumentNullException.ThrowIfNull(operadorId);

        if (preguntasOrdenadas.Count == 0)
            throw new DomainException(
                "La sesión trivia requiere al menos una pregunta.");

        var etapaTrivia = EtapaTriviaSnapshot.Rehydrate(
            EtapaId.Nuevo(),
            1,
            [],
            preguntasOrdenadas,
            categoriasTitulo);

        var snapshot = MisionSnapshot.Rehydrate(
            MisionId.Nuevo(),
            categoriasTitulo,
            [etapaTrivia]);

        return CrearDesdeMision(snapshot, operadorId);
    }

    public void AbrirParaRegistro()
    {
        if (Estado != EstadoSesion.Programada)
            throw new DomainException(
                $"No se puede abrir para registro una sesión en estado '{Estado}'. " +
                "Solo es posible desde 'Programada'.");

        Estado = EstadoSesion.EnPreparacion;
        RegistrarEvento("SesionAbiertaParaRegistro", string.Empty);
    }

    public ParticipanteSesion UnirseParticipante(UsuarioId jugadorId, string nombre, string codigoAccesoIngresado)
    {
        if (Estado is EstadoSesion.Finalizada or EstadoSesion.Cancelada)
            throw new DomainException(
                "No se pueden unir participantes a una sesión cerrada.");

        if (Estado is EstadoSesion.Activa or EstadoSesion.Pausada)
            throw new DomainException(
                "La sesión ya está en juego. Solo puedes unirte antes de que el operador la inicie.");

        if (!CodigoAcceso.CoincideCon(codigoAccesoIngresado))
            throw new DomainException("El código de acceso de la sesión no es válido.");

        if (Estado == EstadoSesion.Programada)
            AbrirParaRegistro();

        if (_participantes.Any(e => e.JugadorId == jugadorId))
            throw new DomainException("Ya estás inscrito en esta sesión.");

        var nombreParticipante = NombreParticipante.Crear(nombre);

        if (_participantes.Any(e =>
                string.Equals(e.Nombre.Valor, nombreParticipante.Valor, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException(
                $"Ya existe un participante con el nombre '{nombreParticipante.Valor}' en esta sesión.");

        var participante = ParticipanteSesion.Crear(SesionId, jugadorId, nombreParticipante.Valor);
        _participantes.Add(participante);
        RegistrarEvento("ParticipanteUnido", nombreParticipante.Valor);
        return participante;
    }

    public void Iniciar()
    {
        if (Estado != EstadoSesion.EnPreparacion)
            throw new DomainException(
                $"No se puede iniciar una sesión en estado '{Estado}'. " +
                "Solo es posible desde 'EnPreparacion'.");

        if (!_participantes.Any())
            throw new DomainException(
                "La sesión necesita al menos un participante registrado.");

        Estado     = EstadoSesion.Activa;
        IniciadaEn = DateTime.UtcNow;
        RaiseDomainEvent(new SesionIniciada(SesionId, TipoSesion));
        RegistrarEvento("SesionIniciada", $"operador={OperadorId.Valor}");
    }

    public void Pausar()
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                $"No se puede pausar una sesión en estado '{Estado}'.");

        Estado = EstadoSesion.Pausada;
        RaiseDomainEvent(new Events.SesionPausada(SesionId));
        RegistrarEvento("SesionPausada", string.Empty);
    }

    public void Reanudar()
    {
        if (Estado != EstadoSesion.Pausada)
            throw new DomainException(
                $"No se puede reanudar una sesión en estado '{Estado}'.");

        Estado = EstadoSesion.Activa;
        RaiseDomainEvent(new Events.SesionReanudada(SesionId));
        RegistrarEvento("SesionReanudada", string.Empty);
    }

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

        var motivoLimpio = motivo.Trim();
        Estado       = EstadoSesion.Cancelada;
        FinalizadaEn = DateTime.UtcNow;
        RaiseDomainEvent(new Events.SesionCancelada(SesionId, motivoLimpio));
        RegistrarEvento("SesionCancelada", motivoLimpio);
    }

    public IReadOnlyList<PosicionRanking> ObtenerRankingFinal()
    {
        if (Estado is not (EstadoSesion.Finalizada or EstadoSesion.Cancelada))
            throw new DomainException(
                "Solo se puede obtener el ranking de sesiones finalizadas o canceladas.");

        return RankingService.Calcular(Participantes);
    }

    public void AplicarPenalizacion(ParticipanteId participanteId, Penalizacion penalizacion)
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                "Solo se pueden aplicar penalizaciones en sesiones activas.");

        var participante = ObtenerParticipante(participanteId);
        participante.AplicarPenalizacion(penalizacion);

        RaiseDomainEvent(new Events.PenalizacionAplicada(
            SesionId, participanteId,
            penalizacion.Puntos, penalizacion.Motivo,
            penalizacion.OperadorId));

        RegistrarEvento("PenalizacionAplicada",
            $"participante={participanteId.Valor};puntos={penalizacion.Puntos};motivo={penalizacion.Motivo}");
    }

    public Evidencia RegistrarEvidencia(ParticipanteId participanteId, string codigoQR)
    {
        if (ContextoMision is null)
            throw new DomainException(
                "Solo las sesiones de misión aceptan evidencias QR.");

        if (ContextoMision.ObtenerEtapaActual() is not EtapaBusquedaTesoroSnapshot)
            throw new DomainException(
                "Solo se aceptan evidencias en etapas de Búsqueda del Tesoro.");

        var participante = ObtenerParticipante(participanteId);
        var qr     = CodigoQR.Crear(codigoQR);
        var etapa  = ContextoMision.ObtenerEtapaBusquedaTesoroActual();

        var resultado = ValidacionEvidenciaService.Validar(this, qr);

        if (resultado == ResultadoValidacion.Valida &&
            _evidencias.Any(e =>
                e.ParticipanteId == participanteId &&
                e.EtapaId == etapa.EtapaId &&
                e.Resultado == ResultadoValidacion.Valida))
        {
            resultado = ResultadoValidacion.Invalida;
        }

        if (resultado == ResultadoValidacion.Valida &&
            ContextoMision.YaHayGanadorEnEtapaActual())
        {
            resultado = ResultadoValidacion.Invalida;
        }

        var evidencia = Evidencia.Registrar(
            SesionId, participante.ParticipanteId, etapa.EtapaId, qr, resultado);
        _evidencias.Add(evidencia);

        RaiseDomainEvent(new EvidenciaRegistrada(
            SesionId, participante.ParticipanteId, etapa.EtapaId, resultado, qr.Valor));

        RegistrarEvento("EvidenciaRegistrada",
            $"participante={participante.ParticipanteId.Valor};etapa={etapa.EtapaId.Valor};resultado={resultado};qr={qr.Valor}");

        if (resultado == ResultadoValidacion.Valida)
            ProcesarEvidenciaGanadora(participante, etapa);

        return evidencia;
    }

    private void ProcesarEvidenciaGanadora(ParticipanteSesion participante, EtapaBusquedaTesoroSnapshot etapa)
    {
        var puntos = CalculoPuntajeBusquedaService.Calcular(esGanador: true);
        participante.SumarPuntaje(puntos.Valor);

        ContextoMision!.RegistrarGanadorEtapa(participante.ParticipanteId);

        RaiseDomainEvent(new EvidenciaValidada(
            SesionId, participante.ParticipanteId, etapa.EtapaId, puntos));

        var indexCompletada = ContextoMision.EtapaActualIndex;
        var esUltima        = ContextoMision.EsUltimaEtapa();

        RaiseDomainEvent(new EtapaCompletada(
            SesionId, indexCompletada, participante.ParticipanteId));

        RegistrarEvento("EtapaCompletada",
            $"etapaIndex={indexCompletada};ganador={participante.ParticipanteId.Valor}");

        if (esUltima)
            Finalizar();
        else
            ContextoMision.AvanzarEtapa();
    }

    public bool EstaActiva() => Estado == EstadoSesion.Activa;

    private ParticipanteSesion ObtenerParticipante(ParticipanteId participanteId) =>
        _participantes.FirstOrDefault(e => e.ParticipanteId == participanteId)
        ?? throw new DomainException(
            $"El participante '{participanteId.Valor}' no pertenece a esta sesión.");

    private void RegistrarEvento(string tipo, string payload) =>
        _historialEventos.Add(EventoSesion.Crear(SesionId, tipo, payload));

    protected override bool IdEquals(Entity other) =>
        other is Sesion s && s.SesionId == SesionId;

    protected override int GetIdHashCode() => SesionId.GetHashCode();
}
