/** Respuesta de GET /usuarios y GET /usuarios/{id}. No incluye contraseña. */
export interface UsuarioDto {
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

/** Roles asignables desde el panel de administración (HU-42 / RB-35). */
export const ROLES_USUARIO = ['Administrador', 'Operador'] as const
export type RolUsuarioAdmin = (typeof ROLES_USUARIO)[number]
