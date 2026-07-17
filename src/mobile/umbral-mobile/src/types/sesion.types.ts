export interface SesionDisponibleParticipanteDto {
  id: string
  titulo: string
  estado: string
  participantesInscritos: number
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
}

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
  etapaIniciadaEn?: string | null
  segundosPausaAcumulados?: number
  pausadaDesde?: string | null
  pistasPorTiempoPendientes?: PistaPorTiempoPendienteDto[] | null
  penalizaciones?: PenalizacionParticipanteDto[] | null
}

export interface UnirseSesionRequest {
  codigoAcceso: string
  nombreParticipante?: string
}

export interface UnirseSesionResponse {
  participanteId: string
}

export interface PosicionRankingDto {
  posicion: number
  participanteId: string
  nombreParticipante: string
  puntajeTotal: number
  tiempoAcumuladoMs?: number
}

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

export interface SubmitRespuestaTriviaResponse {
  messageId: string
  status: string
}

export interface TriviaRespuestaFeedback {
  esCorrecta: boolean
  fueraDeTiempo: boolean
  puntosOtorgados: number
  puntajeTotal: number
}

export interface SubmitEvidenciaRequest {
  codigoQr: string
}

export interface SubmitEvidenciaResponse {
  evidenciaId: string
  resultado: string
}

export interface ParticipanteSesionInscrita {
  sesionId: string
  titulo: string
  participanteId: string
  joinedAt: string
}
