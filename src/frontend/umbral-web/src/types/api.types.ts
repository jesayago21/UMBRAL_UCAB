export type RolUsuario = 'Administrador' | 'Operador' | 'EquipoParticipante'

export interface ApiErrorResponse {
  tipo: string
  mensaje: string
  errores?: Record<string, string[]>
  traceId?: string
}

export interface CreatedIdResponse {
  id: string
}
