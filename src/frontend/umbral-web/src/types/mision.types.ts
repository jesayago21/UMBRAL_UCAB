export type EstadoMision = 'Activa' | 'Inactiva'
export type TipoEtapaApi = 'BusquedaTesoro' | 'Trivia'

export interface PistaDto {
  pistaId: string
  contenido: string
  tipoLiberacion: string
  segundosLiberacion: number | null
}

export interface EtapaDto {
  etapaId: string
  orden: number
  tipoEtapa: TipoEtapaApi
  descripcion?: string | null
  codigoQrSolucion?: string | null
  pistas?: PistaDto[] | null
  categoriaIds?: string[] | null
  latitud?: number | null
  longitud?: number | null
  radioMetros?: number | null
}

export interface MisionDto {
  id: string
  nombre: string
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
  tipoEtapa: TipoEtapaApi
  orden: number
  descripcion?: string
  codigoQrSolucion?: string
  pistas?: CrearPistaRequest[]
  categoriaIds?: string[]
  latitud?: number | null
  longitud?: number | null
  radioMetros?: number | null
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

export interface AgregarEtapaMisionRequest {
  tipoEtapa: TipoEtapaApi
  descripcion?: string
  codigoQrSolucion?: string
  categoriaIds?: string[]
  pistas?: CrearPistaRequest[]
  latitud?: number | null
  longitud?: number | null
  radioMetros?: number | null
}

export interface EditarEtapaMisionRequest {
  descripcion?: string | null
  codigoQrSolucion?: string | null
  categoriaIds?: string[] | null
  latitud?: number | null
  longitud?: number | null
  radioMetros?: number | null
}

export interface AgregarPistaEtapaRequest {
  contenido: string
  tipoLiberacion: string
  segundosLiberacion?: number | null
}

export type EditarPistaEtapaRequest = AgregarPistaEtapaRequest

export interface ListMisionesParams {
  nombre?: string
  estado?: string
}
