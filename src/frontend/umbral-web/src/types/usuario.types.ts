/** Respuesta de GET /usuarios y GET /usuarios/{id}. No incluye contraseña. */
export interface UsuarioDto {
  id: string
  keycloakUserId: string
  email: string
  username: string
  nombre: string
  apellido: string
  estado: string
  roles: string[]
}

/** Respuesta exclusiva del POST 201. Incluye contraseña temporal para entrega inmediata. */
export interface CrearUsuarioResponse {
  id: string
  keycloakUserId: string
  email: string
  username: string
  nombre: string
  apellido: string
  estado: string
  roles: string[]
  passwordTemporal: string
}

export interface CrearUsuarioRequest {
  email: string
  username: string
  nombre: string
  apellido: string
  passwordTemporal: string
  roles: [string]
}

export interface AsignarRolesRequest {
  roles: string[]
}

export interface ActualizarUsuarioRequest {
  nombre: string
  apellido: string
  rol: string
  nuevaPassword?: string | null
}

export const ROLES_USUARIO = ['Administrador', 'Operador', 'Participante'] as const
export type RolUsuarioAdmin = (typeof ROLES_USUARIO)[number]
