export type EstadoMision = 'Activa' | 'Inactiva'

export interface PistaDto {
  pistaId: string
  contenido: string
  tipoLiberacion: string
  segundosLiberacion: number | null
}

export interface EtapaDto {
  etapaId: string
  orden: number
  descripcion: string
  codigoQrSolucion: string
  pistas: PistaDto[]
}

export interface MisionDto {
  id: string
  nombre: string
  descripcion: string
  nivelDificultad: string
  tiempoMaximoSeg: number
  estado: EstadoMision
  totalEtapas: number
  etapas: EtapaDto[]
}

export interface CrearPistaRequest {
  contenido: string
  tipoLiberacion: string
  segundosLiberacion?: number | null
}

export interface CrearEtapaRequest {
  descripcion: string
  codigoQrSolucion: string
  pistas: CrearPistaRequest[]
}

export interface CrearMisionRequest {
  nombre: string
  etapas: CrearEtapaRequest[]
  activar?: boolean
}

export interface ActualizarMisionRequest {
  nombre: string
  activar?: boolean | null
}

export interface ListMisionesParams {
  nombre?: string
  estado?: string
}
