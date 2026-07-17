export type RolUsuario = 'Administrador' | 'Operador' | 'Participante'

export interface ApiErrorResponse {
  mensaje?: string
  errores?: Record<string, string[]>
}
