export type EstadoSesionUi =
  | 'programada'
  | 'enPreparacion'
  | 'activa'
  | 'pausada'
  | 'finalizada'
  | 'cancelada'

export type TipoSesionApi = 'BusquedaTesoro' | 'Trivia' | 'Mision'

export interface SesionResumenDto {
  id: string
  nombre: string
  tipoSesion: TipoSesionApi
  misionId: string
  misionNombre: string
  estado: string
  participantesCount: number
  iniciadaEn: string | null
  finalizadaEn: string | null
  etapaActualOrden: number
  totalEtapas: number
  etapaActualDescripcion: string | null
  etapaActivaTipo?: string | null
}

export interface PistaSesionDto {
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
}

export interface SesionDetalleDto extends SesionResumenDto {
  codigoAcceso: string
  participantes: ParticipanteSesionDto[]
  etapas: EtapaSesionDto[] | null
}

export interface SesionDisponibleParticipanteDto {
  id: string
  titulo: string
  estado: string
  participantesInscritos: number
}

export interface MiInscripcionParticipanteDto {
  sesionId: string
  titulo: string
  participanteId: string
  estado: string
  totalEtapas?: number
  etapas?: EtapaSesionDto[]
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
