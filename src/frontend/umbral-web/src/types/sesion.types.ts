export type EstadoSesionUi =
  | 'programada'
  | 'enPreparacion'
  | 'activa'
  | 'pausada'
  | 'finalizada'
  | 'cancelada'

export type TipoSesionApi = 'BusquedaTesoro' | 'Trivia' | 'Mision'

/** Cupo máximo por sesión (alineado con Sesion.MaxParticipantes en dominio). */
export const MAX_PARTICIPANTES_SESION = 5

export interface SesionResumenDto {
  id: string
  nombre: string
  tipoSesion: TipoSesionApi
  misionId: string
  misionNombre: string
  estado: string
  participantesCount: number
  maxParticipantes?: number
  iniciadaEn: string | null
  finalizadaEn: string | null
  etapaActualOrden: number
  totalEtapas: number
  etapaActualDescripcion: string | null
  etapaActivaTipo?: string | null
}

export interface PistaSesionDto {
  pistaId: string
  contenido: string
  tipoLiberacion: string
  segundosLiberacion: number | null
}

export interface EtapaSesionDto {
  orden: number
  tipoEtapa: string
  descripcion: string
  esActual: boolean
  pistas: PistaSesionDto[] | null
  categoriaIds?: string[] | null
  latitud?: number | null
  longitud?: number | null
  radioMetros?: number | null
  /** Solo en detalle de operador; no llega al participante. */
  codigoQrSolucion?: string | null
}

export interface SesionDetalleDto extends SesionResumenDto {
  codigoAcceso: string
  participantes: ParticipanteSesionDto[]
  etapas: EtapaSesionDto[] | null
  /** HU-33 — Esperando | PreguntaActiva | Transicion */
  triviaFase?: string | null
}

export interface SesionDisponibleParticipanteDto {
  id: string
  titulo: string
  estado: string
  participantesInscritos: number
  maxParticipantes?: number
}

/** Countdown PorTiempo sin revelar contenido (HU-17 / RB-07). */
export interface PistaPorTiempoPendienteDto {
  pistaId: string
  segundosLiberacion: number
}

export interface PenalizacionParticipanteDto {
  puntos: number
  motivo: string
  ocurridoEn: string
}

export interface MiInscripcionParticipanteDto {
  sesionId: string
  titulo: string
  participanteId: string
  estado: string
  totalEtapas?: number
  etapas?: EtapaSesionDto[]
  /** UTC — inicio del reloj PorTiempo de la etapa BT actual. */
  etapaIniciadaEn?: string | null
  segundosPausaAcumulados?: number
  /** UTC — si la sesión está pausada, desde cuándo (congela el countdown). */
  pausadaDesde?: string | null
  pistasPorTiempoPendientes?: PistaPorTiempoPendienteDto[] | null
  /** Penalizaciones aplicadas a este participante (con motivo). */
  penalizaciones?: PenalizacionParticipanteDto[] | null
  participantesInscritos?: number
  maxParticipantes?: number
}

export interface SesionEtapasParticipanteDto {
  estado: string
  totalEtapas: number
  etapas: EtapaSesionDto[]
}

export interface OperadorSesionState {
  sesionId: string
  /** Ausente en sessionStorage antiguo → se asume búsqueda del tesoro */
  tipoSesion?: TipoSesionApi
  misionId: string
  /** Nombre de la instancia de sesión (visible en listados). */
  nombre: string
  misionNombre: string
  estado: EstadoSesionUi
  codigoAcceso: string
  participantes: ParticipanteSesionDto[]
  iniciadaEn?: string | null
  finalizadaEn?: string | null
  etapaActualOrden?: number
  totalEtapas?: number
  etapaDescripcion?: string | null
}

export interface ParticipanteSesionDto {
  participanteId: string
  jugadorId?: string
  nombre: string
}

export interface CrearSesionBusquedaTesoroRequest {
  misionId: string
}

export interface CrearSesionBusquedaTesoroResponse {
  id: string
  codigoAcceso: string
}

export interface CrearSesionTriviaRequest {
  categoriaIds: string[]
}

export type CrearSesionTriviaResponse = CrearSesionBusquedaTesoroResponse

export interface UnirseSesionRequest {
  codigoAcceso: string
  nombreParticipante?: string
}

export interface UnirseSesionResponse {
  participanteId: string
}

export interface CancelarSesionRequest {
  motivo: string
}

export interface PosicionRankingDto {
  posicion: number
  participanteId: string
  nombreParticipante: string
  puntajeTotal: number
  /** HU-39 — suma de tiempos útiles (ms); desempate RB-08. */
  tiempoAcumuladoMs?: number
}

export interface MisionActivaDto {
  id: string
  nombre: string
}

export interface PreguntaTriviaParticipanteDto {
  orden: number
  id: string
  enunciado: string
  dificultad: string
  opciones: string[]
}

/** HU-33 — ronda trivia sincronizada. */
export type FaseTriviaUi = 'Esperando' | 'PreguntaActiva' | 'Transicion'

export interface EstadoTriviaSesionDto {
  fase: string
  orden: number | null
  preguntaId: string | null
  enunciado: string | null
  dificultad: string | null
  opciones: string[] | null
  timerCerradoEnUtc: string | null
  transicionHastaUtc: string | null
  preguntaIndexActual: number
  totalPreguntas: number
  yaRespondio: boolean
  indiceOpcionSeleccionada: number | null
  ultimaRespuestaEsCorrecta?: boolean | null
  ultimaRespuestaFueraDeTiempo?: boolean | null
  ultimaRespuestaPuntos?: number | null
  puntajeTotalParticipante?: number | null
}

export interface SubmitRespuestaTriviaRequest {
  preguntaId: string
  indiceOpcion: number
  duracionTimerSegundos?: number
}

/** 202 Accepted — respuesta encolada (HU-35). */
export interface SubmitRespuestaTriviaResponse {
  messageId: string
  status: string
}

/** Feedback mostrado al participante tras procesar la cola. */
export interface TriviaRespuestaFeedback {
  esCorrecta: boolean
  fueraDeTiempo: boolean
  puntosOtorgados: number
  puntajeTotal: number
}

export interface AplicarPenalizacionRequest {
  participanteId: string
  puntos: number
  motivo: string
}

/** RF-15 — pista ad-hoc. `participanteId` null = todos. */
export interface LiberarPistaManualRequest {
  contenido: string
  participanteId?: string | null
}

export interface SubmitEvidenciaRequest {
  codigoQr: string
}

export interface SubmitEvidenciaResponse {
  evidenciaId: string
  resultado: string
}

export interface EventoSesionDto {
  eventoId: string
  tipo: string
  payload: string
  ocurridoEn: string
}

export interface HistorialSesionDto {
  sesionId: string
  pagina: number
  tamanoPagina: number
  totalEventos: number
  eventos: EventoSesionDto[]
}

export interface ReporteFinalSesionDto {
  sesionId: string
  estado: string
  finalizadaEn: string | null
  ranking: PosicionRankingDto[]
}
