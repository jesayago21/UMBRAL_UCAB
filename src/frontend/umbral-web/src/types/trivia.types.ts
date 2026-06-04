export type DificultadPregunta = 'Facil' | 'Media' | 'Dificil'

export interface CategoriaDto {
  id: string
  nombre: string
}

export interface OpcionRespuestaDto {
  texto: string
  esCorrecta: boolean
}

export interface PreguntaDto {
  id: string
  enunciado: string
  dificultad: DificultadPregunta | string
  categoriaId: string | null
  opciones: OpcionRespuestaDto[]
}

export interface CrearCategoriaRequest {
  nombre: string
}

export interface ActualizarCategoriaRequest {
  nombre: string
}

export interface CrearPreguntaRequest {
  enunciado: string
  dificultad: string
  categoriaId?: string | null
  opciones: OpcionRespuestaDto[]
}

export interface ActualizarPreguntaRequest {
  enunciado: string
  dificultad: string
  categoriaId?: string | null
  opciones: OpcionRespuestaDto[]
}

export interface ListPreguntasParams {
  categoriaId?: string
  dificultad?: string
  enunciado?: string
}

export interface ListCategoriasParams {
  nombre?: string
}
