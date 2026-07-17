using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Shared;

namespace Umbral.Domain.Sesion;

/// <summary>
/// Contexto unificado de ejecución de una sesión de misión (etapas polimórficas secuenciales).
/// </summary>
public sealed class ContextoMision : Entity
{
    private readonly Guid _id = Guid.NewGuid();
    private readonly List<PistaEntregada> _pistasEntregadas = [];

    public MisionId MisionId { get; private set; } = default!;
    public MisionSnapshot MisionSnapshot { get; private set; } = default!;
    public int EtapaActualIndex { get; private set; }
    public ParticipanteId? GanadorEtapaActualId { get; private set; }
    public int PreguntaTriviaActualIndex { get; private set; }

    /// <summary>UTC — cierre del timer de la pregunta trivia activa (HU-33).</summary>
    public DateTime? TimerCerradoEn { get; private set; }

    /// <summary>Entre preguntas: UI muestra “Preparando siguiente pregunta…” (HU-33).</summary>
    public bool TriviaEnTransicion { get; private set; }

    /// <summary>UTC — fin de la transición; luego el sistema lanza la siguiente (HU-38).</summary>
    public DateTime? TransicionHasta { get; private set; }

    /// <summary>UTC — momento en que comenzó la etapa BT actual (reloj PorTiempo).</summary>
    public DateTimeOffset? EtapaIniciadaEn { get; private set; }

    /// <summary>UTC — inicio de la pausa actual; null si no está pausada.</summary>
    public DateTimeOffset? PausadaDesde { get; private set; }

    /// <summary>Segundos de pausa acumulados desde <see cref="EtapaIniciadaEn"/>.</summary>
    public int SegundosPausaAcumulados { get; private set; }

    public IReadOnlyList<PistaEntregada> PistasEntregadas
    {
        get => _pistasEntregadas;
        private set
        {
            _pistasEntregadas.Clear();
            if (value is { Count: > 0 })
                _pistasEntregadas.AddRange(value);
        }
    }

    private ContextoMision() { }

    internal static ContextoMision Crear(MisionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.Etapas.Count == 0)
            throw new DomainException(
                "La misión debe tener al menos una etapa para iniciar una sesión.");

        return new ContextoMision
        {
            MisionId                  = snapshot.MisionId,
            MisionSnapshot            = snapshot,
            EtapaActualIndex          = 0,
            PreguntaTriviaActualIndex = 0,
            TimerCerradoEn            = null,
            TriviaEnTransicion        = false,
            TransicionHasta           = null
        };
    }

    internal static ContextoMision Rehydrate(
        MisionId misionId,
        MisionSnapshot snapshot,
        int etapaActualIndex,
        ParticipanteId? ganadorEtapaActualId,
        int preguntaTriviaActualIndex,
        DateTimeOffset? etapaIniciadaEn = null,
        DateTimeOffset? pausadaDesde = null,
        int segundosPausaAcumulados = 0,
        IReadOnlyList<PistaEntregada>? pistasEntregadas = null,
        DateTime? timerCerradoEn = null,
        bool triviaEnTransicion = false,
        DateTime? transicionHasta = null)
    {
        var ctx = new ContextoMision
        {
            MisionId                  = misionId,
            MisionSnapshot            = snapshot,
            EtapaActualIndex          = etapaActualIndex,
            GanadorEtapaActualId      = ganadorEtapaActualId,
            PreguntaTriviaActualIndex = preguntaTriviaActualIndex,
            EtapaIniciadaEn           = etapaIniciadaEn,
            PausadaDesde              = pausadaDesde,
            SegundosPausaAcumulados   = segundosPausaAcumulados,
            TimerCerradoEn            = timerCerradoEn,
            TriviaEnTransicion        = triviaEnTransicion,
            TransicionHasta           = transicionHasta
        };

        if (pistasEntregadas is { Count: > 0 })
            ctx._pistasEntregadas.AddRange(pistasEntregadas);

        return ctx;
    }

    public EtapaSnapshotBase ObtenerEtapaActual()
    {
        if (EtapaActualIndex < 0 || EtapaActualIndex >= MisionSnapshot.Etapas.Count)
            throw new DomainException("No hay etapa activa en el contexto de la sesión.");

        return MisionSnapshot.Etapas[EtapaActualIndex];
    }

    public EtapaBusquedaTesoroSnapshot ObtenerEtapaBusquedaTesoroActual()
    {
        var etapa = ObtenerEtapaActual();
        if (etapa is not EtapaBusquedaTesoroSnapshot bt)
            throw new DomainException("La etapa activa no es de Búsqueda del Tesoro.");

        return bt;
    }

    public bool EsUltimaEtapa() =>
        EtapaActualIndex >= MisionSnapshot.Etapas.Count - 1;

    public bool YaHayGanadorEnEtapaActual() => GanadorEtapaActualId is not null;

    public bool YaEntregoPista(PistaId pistaId, int etapaIndex, ParticipanteId participanteId) =>
        _pistasEntregadas.Any(p =>
            p.PistaId == pistaId &&
            p.EtapaIndex == etapaIndex &&
            p.ParticipanteId == participanteId);

    /// <summary>
    /// Segundos efectivos transcurridos en la etapa BT (excluye pausas).
    /// </summary>
    public double SegundosEfectivosTranscurridos(DateTimeOffset ahora)
    {
        if (EtapaIniciadaEn is null)
            return 0;

        var bruto = (ahora - EtapaIniciadaEn.Value).TotalSeconds;
        var pausaActual = PausadaDesde is { } desde
            ? Math.Max(0, (ahora - desde).TotalSeconds)
            : 0;

        return Math.Max(0, bruto - SegundosPausaAcumulados - pausaActual);
    }

    internal void RegistrarGanadorEtapa(ParticipanteId ganadorId)
    {
        ArgumentNullException.ThrowIfNull(ganadorId);

        if (GanadorEtapaActualId is not null)
            throw new DomainException("Ya existe un ganador en la etapa actual.");

        GanadorEtapaActualId = ganadorId;
    }

    internal void AvanzarEtapa(DateTimeOffset ahora)
    {
        if (ObtenerEtapaActual() is EtapaBusquedaTesoroSnapshot && GanadorEtapaActualId is null)
            throw new DomainException(
                "No se puede avanzar de etapa BT sin un ganador registrado.");

        if (EsUltimaEtapa())
            throw new DomainException("No hay más etapas en la misión.");

        EtapaActualIndex++;
        GanadorEtapaActualId = null;
        PreguntaTriviaActualIndex = 0;
        TimerCerradoEn = null;
        TriviaEnTransicion = false;
        TransicionHasta = null;
        ReiniciarRelojEtapa(ahora);
    }

    public FaseTrivia ObtenerFaseTrivia()
    {
        if (TimerCerradoEn is not null)
            return FaseTrivia.PreguntaActiva;

        if (TriviaEnTransicion)
            return FaseTrivia.Transicion;

        return FaseTrivia.Esperando;
    }

    public EtapaTriviaSnapshot ObtenerEtapaTriviaActual()
    {
        var etapa = ObtenerEtapaActual();
        if (etapa is not EtapaTriviaSnapshot trivia)
            throw new DomainException("La etapa activa no es de Trivia.");

        return trivia;
    }

    public PreguntaId ObtenerPreguntaTriviaActualId()
    {
        var trivia = ObtenerEtapaTriviaActual();
        if (PreguntaTriviaActualIndex < 0
            || PreguntaTriviaActualIndex >= trivia.PreguntasOrdenadas.Count)
            throw new DomainException("No hay pregunta de trivia activa en el índice actual.");

        return trivia.PreguntasOrdenadas[PreguntaTriviaActualIndex];
    }

    /// <summary>HU-33 — abre el timer de la pregunta en el índice actual.</summary>
    /// <param name="inicioTimerTrasTransicionUtc">
    /// Si se lanza al terminar la transición entre preguntas, ancla el timer al fin programado
    /// de la transición para no descontar esos segundos de la pregunta siguiente.
    /// </param>
    internal void LanzarPreguntaTrivia(
        DateTimeOffset ahora,
        int duracionSegundos,
        DateTime? inicioTimerTrasTransicionUtc = null)
    {
        if (ObtenerEtapaActual() is not EtapaTriviaSnapshot)
            throw new DomainException("Solo se puede lanzar preguntas en una etapa Trivia.");

        if (duracionSegundos <= 0)
            throw new DomainException("La duración del timer de trivia debe ser mayor a cero.");

        if (TimerCerradoEn is not null)
            throw new DomainException(
                "Ya hay una pregunta de trivia activa. Ciérrala antes de lanzar otra.");

        _ = ObtenerPreguntaTriviaActualId();

        var inicioTimer = inicioTimerTrasTransicionUtc is { } finTransicion
            ? (finTransicion > ahora.UtcDateTime ? finTransicion : ahora.UtcDateTime)
            : ahora.UtcDateTime;

        TriviaEnTransicion = false;
        TransicionHasta = null;
        TimerCerradoEn = inicioTimer.AddSeconds(duracionSegundos);
    }

    /// <summary>HU-33 / HU-38 — cierra la ronda y programa el fin de transición.</summary>
    internal void CerrarPreguntaTriviaYEntrarTransicion(
        DateTimeOffset ahora,
        int duracionTransicionSegundos = 5)
    {
        if (ObtenerEtapaActual() is not EtapaTriviaSnapshot)
            throw new DomainException("Solo se puede cerrar preguntas en una etapa Trivia.");

        if (TimerCerradoEn is null)
            throw new DomainException("No hay una pregunta de trivia activa para cerrar.");

        if (duracionTransicionSegundos < 0)
            throw new DomainException("La duración de transición no puede ser negativa.");

        TimerCerradoEn = null;
        TriviaEnTransicion = true;
        TransicionHasta = ahora.UtcDateTime.AddSeconds(duracionTransicionSegundos);
    }

    /// <summary>
    /// Agota la secuencia de la etapa (sin más preguntas).
    /// No exige respuestas: quien no contestó simplemente no suma puntaje (HU-34).
    /// </summary>
    internal void CompletarSecuenciaTrivia()
    {
        TimerCerradoEn = null;
        TriviaEnTransicion = false;
        TransicionHasta = null;
        PreguntaTriviaActualIndex = 0;
    }

    /// <summary>
    /// HU-33 — avanza el cursor a la siguiente pregunta (solo en transición).
    /// </summary>
    /// <returns><c>true</c> si quedó otra pregunta; <c>false</c> si se agotó la etapa.</returns>
    internal bool AvanzarPreguntaTrivia()
    {
        if (ObtenerEtapaActual() is not EtapaTriviaSnapshot trivia)
            throw new DomainException("Solo se puede avanzar preguntas en una etapa Trivia.");

        if (!TriviaEnTransicion || TimerCerradoEn is not null)
            throw new DomainException(
                "Solo se puede avanzar de pregunta durante la transición entre rondas.");

        if (PreguntaTriviaActualIndex >= trivia.PreguntasOrdenadas.Count - 1)
            return false;

        PreguntaTriviaActualIndex++;
        return true;
    }

    public bool TimerPreguntaVencido(DateTimeOffset ahora) =>
        TimerCerradoEn is { } fin && ahora.UtcDateTime >= fin;

    public bool TransicionVencida(DateTimeOffset ahora) =>
        TriviaEnTransicion
        && TransicionHasta is { } hasta
        && ahora.UtcDateTime >= hasta;

    /// <summary>Inicia o reinicia el reloj PorTiempo de la etapa actual.</summary>
    internal void ReiniciarRelojEtapa(DateTimeOffset ahora)
    {
        SegundosPausaAcumulados = 0;
        PausadaDesde = null;
        EtapaIniciadaEn = ObtenerEtapaActual() is EtapaBusquedaTesoroSnapshot
            ? ahora
            : null;
    }

    internal void RegistrarPausa(DateTimeOffset ahora)
    {
        if (PausadaDesde is not null)
            return;

        PausadaDesde = ahora;
    }

    internal void RegistrarReanudacion(DateTimeOffset ahora)
    {
        if (PausadaDesde is null)
            return;

        var seg = (int)Math.Max(0, (ahora - PausadaDesde.Value).TotalSeconds);
        SegundosPausaAcumulados += seg;
        PausadaDesde = null;
    }

    internal void RegistrarPistaEntregada(PistaEntregada entrega)
    {
        ArgumentNullException.ThrowIfNull(entrega);

        if (YaEntregoPista(entrega.PistaId, entrega.EtapaIndex, entrega.ParticipanteId))
            throw new DomainException(
                "La pista ya fue entregada a este participante en esta etapa.");

        _pistasEntregadas.Add(entrega);
    }

    protected override bool IdEquals(Entity other) =>
        other is ContextoMision c && c._id == _id;

    protected override int GetIdHashCode() => _id.GetHashCode();
}
