export type EstadoSesionUi =
  | 'programada'
  | 'enPreparacion'
  | 'activa'
  | 'pausada'
  | 'finalizada'
  | 'cancelada'

export type TipoSesionApi = 'BusquedaTesoro' | 'Trivia'

export interface SesionResumenDto {
  id: string
  tipoSesion: TipoSesionApi
  misionId: string
  misionNombre: string
  estado: string
  equiposCount: number
  iniciadaEn: string | null
  finalizadaEn: string | null
  etapaActualOrden: number
  totalEtapas: number
  etapaActualDescripcion: string | null
}

export interface SesionDetalleDto extends SesionResumenDto {
  codigoAcceso: string
  equipos: EquipoSesionDto[]
}

export interface SesionDisponibleEquipoDto {
  id: string
  titulo: string
  estado: string
  equiposInscritos: number
}

export interface OperadorSesionState {
  sesionId: string
  /** Ausente en sessionStorage antiguo → se asume búsqueda del tesoro */
  tipoSesion?: TipoSesionApi
  misionId: string
  misionNombre: string
  estado: EstadoSesionUi
  codigoAcceso: string
  equipos: EquipoSesionDto[]
  iniciadaEn?: string | null
  finalizadaEn?: string | null
  etapaActualOrden?: number
  totalEtapas?: number
  etapaDescripcion?: string | null
}

export interface EquipoSesionDto {
  equipoId: string
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
  nombreEquipo?: string
}

export interface UnirseSesionResponse {
  equipoId: string
}

export interface CancelarSesionRequest {
  motivo: string
}

export interface PosicionRankingDto {
  posicion: number
  equipoId: string
  nombreEquipo: string
  puntajeTotal: number
}

export interface MisionActivaDto {
  id: string
  nombre: string
}
