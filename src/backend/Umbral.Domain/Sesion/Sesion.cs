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
    /// <summary>Cupo máximo de participantes por sesión (inscripción).</summary>
    public const int MaxParticipantes = 5;

    public SesionId SesionId { get; private set; } = default!;
    /// <summary>Nombre visible de la instancia de sesión (p. ej. «Grupo A — mañana»).</summary>
    public string Nombre { get; private set; } = default!;
    public TipoSesion TipoSesion { get; private set; }
    public MisionId? MisionId { get; private set; }
    public UsuarioId OperadorId { get; private set; } = default!;
    public EstadoSesion Estado { get; private set; }
    public CodigoAcceso CodigoAcceso { get; private set; } = default!;
    public DateTime IniciadaEn { get; private set; }
    public DateTime? FinalizadaEn { get; private set; }

    public ContextoMision? ContextoMision { get; private set; }

    /// <summary>Contexto legacy en BD (<c>contextos_bt</c>). Preferir <see cref="ContextoMision"/>.</summary>
    public ContextoBusquedaTesoro? ContextoBT { get; private set; }

    /// <summary>Contexto legacy en BD (<c>contextos_trivia</c>). Preferir <see cref="ContextoMision"/>.</summary>
    public ContextoTrivia? ContextoTrivia { get; private set; }

    private readonly List<ParticipanteSesion> _participantes = [];
    private readonly List<EventoSesion> _historialEventos = [];
    private readonly List<Evidencia> _evidencias = [];
    private readonly List<RespuestaTrivia> _respuestasTrivia = [];

    public IReadOnlyList<ParticipanteSesion> Participantes => _participantes.AsReadOnly();
    public IReadOnlyList<EventoSesion> HistorialEventos => _historialEventos.AsReadOnly();
    public IReadOnlyList<Evidencia> Evidencias => _evidencias.AsReadOnly();
    public IReadOnlyList<RespuestaTrivia> RespuestasTrivia => _respuestasTrivia.AsReadOnly();

    private Sesion() { }

    public static Sesion CrearDesdeMision(MisionSnapshot snapshot, UsuarioId operadorId)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(operadorId);
        return CrearDesdeMision(snapshot, operadorId, snapshot.Nombre);
    }

    public static Sesion CrearDesdeMision(
        MisionSnapshot snapshot,
        UsuarioId operadorId,
        string nombreSesion)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(operadorId);

        if (string.IsNullOrWhiteSpace(nombreSesion))
            throw new DomainException("El nombre de la sesión no puede estar vacío.");

        var sesion = new Sesion
        {
            SesionId       = SesionId.Nuevo(),
            Nombre         = nombreSesion.Trim(),
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

        if (_participantes.Count >= MaxParticipantes)
            throw new DomainException(
                $"La sesión ya alcanzó el máximo de {MaxParticipantes} participantes.");

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

    public ParticipanteId AbandonarParticipante(UsuarioId jugadorId)
    {
        if (Estado is EstadoSesion.Activa or EstadoSesion.Pausada)
            throw new DomainException(
                "No puedes abandonar mientras la sesión está en juego. " +
                "Espera a que finalice o pide al operador que cancele la sesión.");

        var participante = _participantes.FirstOrDefault(p => p.JugadorId == jugadorId)
            ?? throw new DomainException("No estás inscrito en esta sesión.");

        _participantes.Remove(participante);
        RegistrarEvento("ParticipanteAbandono", participante.Nombre.Valor);
        return participante.ParticipanteId;
    }

    /// <summary>
    /// HU-32 — el operador expulsa a un participante de la sala de espera (EnPreparacion).
    /// </summary>
    public ParticipanteId ExpulsarParticipante(ParticipanteId participanteId, string motivo)
    {
        ArgumentNullException.ThrowIfNull(participanteId);

        if (Estado != EstadoSesion.EnPreparacion)
            throw new DomainException(
                $"Solo se puede expulsar participantes en sala de espera (EnPreparacion). " +
                $"Estado actual: '{Estado}'.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("El motivo de expulsión es obligatorio.");

        var motivoLimpio = motivo.Trim();
        var participante = _participantes.FirstOrDefault(p => p.ParticipanteId == participanteId)
            ?? throw new DomainException("El participante no está inscrito en esta sesión.");

        _participantes.Remove(participante);
        RegistrarEvento(
            "ParticipanteExpulsado",
            $"nombre={participante.Nombre.Valor};motivo={motivoLimpio}");
        return participante.ParticipanteId;
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
        ContextoMision?.ReiniciarRelojEtapa(DateTimeOffset.UtcNow);
        RaiseDomainEvent(new SesionIniciada(SesionId, TipoSesion));
        RegistrarEvento("SesionIniciada", $"operador={OperadorId.Valor}");
    }

    public void Pausar()
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                $"No se puede pausar una sesión en estado '{Estado}'.");

        Estado = EstadoSesion.Pausada;
        ContextoMision?.RegistrarPausa(DateTimeOffset.UtcNow);
        RaiseDomainEvent(new Events.SesionPausada(SesionId));
        RegistrarEvento("SesionPausada", string.Empty);
    }

    public void Reanudar()
    {
        if (Estado != EstadoSesion.Pausada)
            throw new DomainException(
                $"No se puede reanudar una sesión en estado '{Estado}'.");

        Estado = EstadoSesion.Activa;
        ContextoMision?.RegistrarReanudacion(DateTimeOffset.UtcNow);
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

        return RankingService.Calcular(Participantes, RespuestasTrivia);
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
            $"participante={participante.Nombre.Valor};puntos={penalizacion.Puntos};motivo={penalizacion.Motivo}");
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
            $"participante={participante.Nombre.Valor};etapa={etapa.Orden};resultado={resultado};qr={qr.Valor}");

        if (resultado == ResultadoValidacion.Valida)
            ProcesarEvidenciaGanadora(participante, etapa);

        return evidencia;
    }

    /// <summary>
    /// HU-34 — confirma una respuesta trivia. Tarde o incorrecta = 0 pts; no se puede cambiar (RB-12/13).
    /// </summary>
    public RespuestaTrivia RegistrarRespuestaTrivia(
        ParticipanteId participanteId,
        Pregunta pregunta,
        int indiceOpcion,
        DateTimeOffset ahora,
        int duracionTimerSegundos = 30)
    {
        ArgumentNullException.ThrowIfNull(pregunta);

        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                "Solo se pueden enviar respuestas de trivia mientras la sesión está activa.");

        if (ContextoMision is null)
            throw new DomainException("La sesión no tiene contexto de misión.");

        if (ContextoMision.ObtenerEtapaActual() is not EtapaTriviaSnapshot)
            throw new DomainException("La etapa activa no es de Trivia.");

        var participante = ObtenerParticipante(participanteId);
        var preguntaId = pregunta.PreguntaId;

        if (_respuestasTrivia.Any(r =>
                r.ParticipanteId == participanteId && r.PreguntaId == preguntaId))
            throw new DomainException(
                "Ya confirmaste una respuesta para esta pregunta. No se puede modificar (RB-13).");

        var preguntaActualId = ContextoMision.ObtenerPreguntaTriviaActualId();
        if (preguntaId != preguntaActualId)
            throw new DomainException(
                "La pregunta enviada no es la pregunta activa de la ronda.");

        if (indiceOpcion < 0 || indiceOpcion >= pregunta.Opciones.Count)
            throw new DomainException("El índice de opción no es válido para esta pregunta.");

        var fase = ContextoMision.ObtenerFaseTrivia();
        if (fase == FaseTrivia.Esperando)
            throw new DomainException("No hay una pregunta de trivia activa para responder.");

        var timerCierre = ContextoMision.TimerCerradoEn;
        var ahoraUtc = ahora.UtcDateTime;

        // Transición o timer vencido → fuera de tiempo (RB-12).
        var fueraDeTiempo = fase != FaseTrivia.PreguntaActiva
                            || ValidacionRespuestaTriviaService.EsFueraDeTiempo(ahoraUtc, timerCierre);

        var esCorrecta = !fueraDeTiempo && pregunta.Opciones[indiceOpcion].EsCorrecta;

        var timerTotalMs = Math.Max(1, duracionTimerSegundos) * 1000L;
        long tiempoRespuestaMs;
        if (timerCierre is { } cierre && fase == FaseTrivia.PreguntaActiva)
        {
            var restanteMs = Math.Max(0, (cierre - ahoraUtc).TotalMilliseconds);
            tiempoRespuestaMs = fueraDeTiempo
                ? timerTotalMs
                : (long)Math.Clamp(timerTotalMs - restanteMs, 0, timerTotalMs);
        }
        else
        {
            tiempoRespuestaMs = timerTotalMs;
        }

        var puntaje = CalculoPuntajeTriviaService.Calcular(
            esCorrecta,
            fueraDeTiempo,
            tiempoRespuestaMs,
            timerTotalMs);

        var respuesta = RespuestaTrivia.Crear(
            SesionId,
            participanteId,
            preguntaId,
            indiceOpcion,
            ahoraUtc,
            fueraDeTiempo,
            esCorrecta,
            puntaje.Valor,
            tiempoRespuestaMs);

        _respuestasTrivia.Add(respuesta);

        if (puntaje.Valor > 0)
            participante.SumarPuntaje(puntaje.Valor);

        RaiseDomainEvent(new RespuestaTriviaRecibida(
            SesionId,
            participanteId,
            preguntaId,
            indiceOpcion,
            esCorrecta,
            fueraDeTiempo,
            puntaje.Valor));

        var enunciadoCorto = pregunta.Enunciado.Length <= 80
            ? pregunta.Enunciado
            : pregunta.Enunciado[..80] + "…";
        // Evita romper el formato clave=valor del historial.
        enunciadoCorto = enunciadoCorto.Replace(';', ',');

        RegistrarEvento(
            "RespuestaTriviaRecibida",
            $"participante={participante.Nombre.Valor};pregunta={enunciadoCorto};" +
            $"indice={indiceOpcion};correcta={esCorrecta};tarde={fueraDeTiempo};puntos={puntaje.Valor}");

        return respuesta;
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
            $"etapaIndex={indexCompletada};ganador={participante.Nombre.Valor}");

        // HU-10: liberar pistas PorGanador de la etapa BT siguiente a los demás, antes de avanzar.
        if (!esUltima)
            LiberarPistasPorGanadorEtapaSiguiente(participante.ParticipanteId, DateTimeOffset.UtcNow);

        if (esUltima)
            Finalizar();
        else
            ContextoMision.AvanzarEtapa(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// HU-10 — libera pistas PorGanador de la etapa BT siguiente a todos los participantes
    /// excepto el ganador (producto: "pistas de avance para los demás"). RB-21 / ERS HU-10.
    /// No-op si la siguiente etapa no es BT o no hay pistas PorGanador.
    /// </summary>
    /// <returns>Cantidad de entregas realizadas.</returns>
    public int LiberarPistasPorGanadorEtapaSiguiente(
        ParticipanteId ganadorId,
        DateTimeOffset ahora)
    {
        if (Estado != EstadoSesion.Activa)
            return 0;

        if (ContextoMision is null)
            return 0;

        if (ContextoMision.EsUltimaEtapa())
            return 0;

        var siguienteIndex = ContextoMision.EtapaActualIndex + 1;
        if (siguienteIndex < 0 || siguienteIndex >= ContextoMision.MisionSnapshot.Etapas.Count)
            return 0;

        if (ContextoMision.MisionSnapshot.Etapas[siguienteIndex] is not EtapaBusquedaTesoroSnapshot siguienteBt)
            return 0;

        var liberadas = 0;

        foreach (var pista in siguienteBt.Pistas)
        {
            if (pista.TipoLiberacion != TipoLiberacion.PorGanador)
                continue;

            foreach (var participante in _participantes)
            {
                if (participante.ParticipanteId == ganadorId)
                    continue;

                if (ContextoMision.YaEntregoPista(
                        pista.PistaId, siguienteIndex, participante.ParticipanteId))
                    continue;

                var entrega = PistaEntregada.Crear(
                    pista.PistaId,
                    siguienteIndex,
                    participante.ParticipanteId,
                    ahora);

                ContextoMision.RegistrarPistaEntregada(entrega);

                RaiseDomainEvent(new PistaLiberada(
                    SesionId,
                    participante.ParticipanteId,
                    pista.PistaId,
                    siguienteIndex,
                    pista.Contenido));

                RegistrarEvento(
                    "PistaLiberada",
                    $"pista={pista.PistaId.Valor};etapaIndex={siguienteIndex};participante={participante.Nombre.Valor};motivo=PorGanador");

                liberadas++;
            }
        }

        return liberadas;
    }

    /// <summary>
    /// Libera pistas PorTiempo vencidas de la etapa BT actual para todos los participantes (RB-07, RB-21, RB-23).
    /// No hace nada si la sesión no está Activa o la etapa actual no es BusquedaTesoro.
    /// El reloj excluye tiempo de pausa (freeze).
    /// </summary>
    /// <returns>Cantidad de entregas realizadas en esta invocación.</returns>
    public int LiberarPistasPorTiempoVencidas(DateTimeOffset ahora)
    {
        if (Estado != EstadoSesion.Activa)
            return 0;

        if (ContextoMision is null)
            return 0;

        if (ContextoMision.ObtenerEtapaActual() is not EtapaBusquedaTesoroSnapshot etapa)
            return 0;

        if (ContextoMision.EtapaIniciadaEn is null)
            return 0;

        var elapsed = ContextoMision.SegundosEfectivosTranscurridos(ahora);
        var etapaIndex = ContextoMision.EtapaActualIndex;
        var liberadas = 0;

        foreach (var pista in etapa.Pistas)
        {
            if (pista.TipoLiberacion != TipoLiberacion.PorTiempo)
                continue;

            if (pista.SegundosLiberacion is null or <= 0)
                continue;

            if (elapsed < pista.SegundosLiberacion.Value)
                continue;

            foreach (var participante in _participantes)
            {
                if (ContextoMision.YaEntregoPista(
                        pista.PistaId, etapaIndex, participante.ParticipanteId))
                    continue;

                var entrega = PistaEntregada.Crear(
                    pista.PistaId,
                    etapaIndex,
                    participante.ParticipanteId,
                    ahora);

                ContextoMision.RegistrarPistaEntregada(entrega);

                RaiseDomainEvent(new PistaLiberada(
                    SesionId,
                    participante.ParticipanteId,
                    pista.PistaId,
                    etapaIndex,
                    pista.Contenido));

                RegistrarEvento(
                    "PistaLiberada",
                    $"pista={pista.PistaId.Valor};etapaIndex={etapaIndex};participante={participante.Nombre.Valor}");

                liberadas++;
            }
        }

        return liberadas;
    }

    /// <summary>
    /// RF-15 — el operador escribe una pista ad-hoc y la entrega a un participante
    /// o a todos. No usa pistas del catálogo; genera un <see cref="PistaId"/> nuevo.
    /// Solo en sesión Activa y etapa BT actual.
    /// </summary>
    /// <returns>Cantidad de entregas realizadas.</returns>
    public int LiberarPistaManual(
        string contenido,
        ParticipanteId? participanteId,
        DateTimeOffset ahora)
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                "Solo se puede liberar pistas manuales mientras la sesión está activa.");

        if (ContextoMision is null)
            throw new DomainException(
                "La sesión no tiene contexto de misión para liberar pistas.");

        if (ContextoMision.ObtenerEtapaActual() is not EtapaBusquedaTesoroSnapshot)
            throw new DomainException(
                "Solo se puede liberar pistas manuales en una etapa de búsqueda del tesoro.");

        var texto = (contenido ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(texto))
            throw new DomainException("El contenido de la pista es obligatorio.");

        IReadOnlyList<ParticipanteSesion> destinatarios;
        if (participanteId is null)
        {
            if (_participantes.Count == 0)
                throw new DomainException(
                    "No hay participantes inscritos para recibir la pista.");

            destinatarios = _participantes;
        }
        else
        {
            destinatarios = [ObtenerParticipante(participanteId)];
        }

        var pistaId = PistaId.Nuevo();
        var etapaIndex = ContextoMision.EtapaActualIndex;
        var liberadas = 0;

        foreach (var participante in destinatarios)
        {
            if (ContextoMision.YaEntregoPista(
                    pistaId, etapaIndex, participante.ParticipanteId))
                continue;

            var entrega = PistaEntregada.Crear(
                pistaId,
                etapaIndex,
                participante.ParticipanteId,
                ahora,
                texto);

            ContextoMision.RegistrarPistaEntregada(entrega);

            RaiseDomainEvent(new PistaLiberada(
                SesionId,
                participante.ParticipanteId,
                pistaId,
                etapaIndex,
                texto));

            RegistrarEvento(
                "PistaLiberada",
                $"pista={pistaId.Valor};etapaIndex={etapaIndex};participante={participante.Nombre.Valor};motivo=Manual");

            liberadas++;
        }

        return liberadas;
    }

    public bool EstaActiva() => Estado == EstadoSesion.Activa;

    /// <summary>
    /// HU-33 — el operador inicia la secuencia automática (solo desde Esperando).
    /// </summary>
    public void IniciarSecuenciaTrivia(DateTimeOffset ahora, int duracionSegundos = 30)
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                "Solo se puede iniciar la trivia mientras la sesión está activa.");

        if (ContextoMision is null)
            throw new DomainException("La sesión no tiene contexto de misión.");

        if (ContextoMision.ObtenerEtapaActual() is not EtapaTriviaSnapshot)
            throw new DomainException("La etapa activa no es de Trivia.");

        if (ContextoMision.ObtenerFaseTrivia() != FaseTrivia.Esperando)
            throw new DomainException(
                "La secuencia de trivia ya está en curso. El avance es automático.");

        ContextoMision.LanzarPreguntaTrivia(ahora, duracionSegundos);

        var preguntaId = ContextoMision.ObtenerPreguntaTriviaActualId();
        RegistrarEvento(
            "PreguntaTriviaIniciada",
            $"pregunta={preguntaId.Valor};index={ContextoMision.PreguntaTriviaActualIndex};" +
            $"timerCerradoEn={ContextoMision.TimerCerradoEn:O}");
    }

    /// <summary>
    /// HU-33 — lanza (o relanza tras transición) la pregunta trivia actual con timer absoluto.
    /// Si está en transición, avanza automáticamente al siguiente índice cuando queda otra pregunta.
    /// </summary>
    public void LanzarPreguntaTrivia(DateTimeOffset ahora, int duracionSegundos = 30)
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                "Solo se pueden lanzar preguntas de trivia mientras la sesión está activa.");

        if (ContextoMision is null)
            throw new DomainException("La sesión no tiene contexto de misión.");

        if (ContextoMision.ObtenerFaseTrivia() == FaseTrivia.Transicion)
        {
            if (!ContextoMision.AvanzarPreguntaTrivia())
                throw new DomainException(
                    "No quedan más preguntas en la etapa Trivia actual.");

            var finTransicion = ContextoMision.TransicionHasta;
            ContextoMision.LanzarPreguntaTrivia(ahora, duracionSegundos, finTransicion);
        }
        else
        {
            ContextoMision.LanzarPreguntaTrivia(ahora, duracionSegundos);
        }

        var preguntaId = ContextoMision.ObtenerPreguntaTriviaActualId();
        RegistrarEvento(
            "PreguntaTriviaIniciada",
            $"pregunta={preguntaId.Valor};index={ContextoMision.PreguntaTriviaActualIndex};" +
            $"timerCerradoEn={ContextoMision.TimerCerradoEn:O}");
    }

    /// <summary>HU-33 — cierra la pregunta activa y entra en transición.</summary>
    public void CerrarPreguntaTriviaYEntrarTransicion(
        DateTimeOffset? ahora = null,
        int duracionTransicionSegundos = 5)
    {
        if (Estado != EstadoSesion.Activa)
            throw new DomainException(
                "Solo se pueden cerrar preguntas de trivia mientras la sesión está activa.");

        if (ContextoMision is null)
            throw new DomainException("La sesión no tiene contexto de misión.");

        ContextoMision.CerrarPreguntaTriviaYEntrarTransicion(
            ahora ?? DateTimeOffset.UtcNow,
            duracionTransicionSegundos);

        RegistrarEvento(
            "TriviaEnTransicion",
            $"index={ContextoMision.PreguntaTriviaActualIndex};" +
            $"hasta={ContextoMision.TransicionHasta:O}");
    }

    /// <summary>
    /// HU-33 / HU-38 — un tick del motor: cierra timer vencido o lanza la siguiente tras transición.
    /// Las respuestas no son requisito: sin contestar = 0 pts y se avanza igual.
    /// </summary>
    public ResultadoCicloTrivia ProcesarCicloTriviaAutomatico(
        DateTimeOffset ahora,
        int duracionPreguntaSegundos = 30,
        int duracionTransicionSegundos = 5)
    {
        if (Estado != EstadoSesion.Activa || ContextoMision is null)
            return ResultadoCicloTrivia.Ninguna;

        if (ContextoMision.ObtenerEtapaActual() is not EtapaTriviaSnapshot)
            return ResultadoCicloTrivia.Ninguna;

        if (ContextoMision.TimerPreguntaVencido(ahora))
        {
            // Cierra aunque nadie haya respondido (HU-34 aún no suma; avance sí).
            CerrarPreguntaTriviaYEntrarTransicion(ahora, duracionTransicionSegundos);
            return ResultadoCicloTrivia.EntroEnTransicion;
        }

        if (ContextoMision.TransicionVencida(ahora))
        {
            var finTransicionProgramado = ContextoMision.TransicionHasta;

            if (ContextoMision.AvanzarPreguntaTrivia())
            {
                ContextoMision.LanzarPreguntaTrivia(
                    ahora,
                    duracionPreguntaSegundos,
                    finTransicionProgramado);
                var preguntaId = ContextoMision.ObtenerPreguntaTriviaActualId();
                RegistrarEvento(
                    "PreguntaTriviaIniciada",
                    $"pregunta={preguntaId.Valor};index={ContextoMision.PreguntaTriviaActualIndex};" +
                    $"timerCerradoEn={ContextoMision.TimerCerradoEn:O}");
                return ResultadoCicloTrivia.SiguientePreguntaLanzada;
            }

            var indexCerrado = ContextoMision.PreguntaTriviaActualIndex;
            ContextoMision.CompletarSecuenciaTrivia();
            RegistrarEvento("TriviaSecuenciaCompletada", $"index={indexCerrado}");

            if (!ContextoMision.EsUltimaEtapa())
            {
                ContextoMision.AvanzarEtapa(ahora);
                RegistrarEvento(
                    "EtapaAvanzada",
                    $"etapaIndex={ContextoMision.EtapaActualIndex};origen=TriviaSecuencia");

                if (ContextoMision.ObtenerEtapaActual() is EtapaTriviaSnapshot)
                {
                    ContextoMision.LanzarPreguntaTrivia(
                        ahora,
                        duracionPreguntaSegundos,
                        finTransicionProgramado);
                    var preguntaId = ContextoMision.ObtenerPreguntaTriviaActualId();
                    RegistrarEvento(
                        "PreguntaTriviaIniciada",
                        $"pregunta={preguntaId.Valor};index={ContextoMision.PreguntaTriviaActualIndex};" +
                        $"timerCerradoEn={ContextoMision.TimerCerradoEn:O}");
                    return ResultadoCicloTrivia.SiguientePreguntaLanzada;
                }

                return ResultadoCicloTrivia.EtapaAvanzada;
            }

            // Paridad con BT (HU-10): última etapa completada → sesión finalizada.
            Finalizar();
            return ResultadoCicloTrivia.SecuenciaCompletada;
        }

        return ResultadoCicloTrivia.Ninguna;
    }

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
